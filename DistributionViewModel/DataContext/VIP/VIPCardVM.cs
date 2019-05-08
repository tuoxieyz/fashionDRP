using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Data;
using System.Transactions;
using Kernel;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls.Data.DataFilter;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;
using System.Data;

namespace DistributionViewModel
{
    public class VIPCardVM : PagedEditSynchronousVM<VIPCard>
    {
        #region 辅助类

        private class VIPCardEntityForSearch : VIPCard
        {
            public int KindID { get; set; }
        }

        #endregion

        #region 属性

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "卡号", PropertyName = "Code", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "姓名", PropertyName = "CustomerName", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "VIP类型", PropertyName = "KindID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "性别", PropertyName = "Sex", PropertyType = typeof(bool)},
                        new ItemPropertyDefinition { DisplayName = "手机号码", PropertyName = "MobilePhone", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "出生日期", PropertyName = "Birthday", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "发卡机构", PropertyName = "OrganizationID", PropertyType = typeof(int)},
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public override CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("CustomerName", FilterOperator.Contains, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, VMGlobal.CurrentUser.OrganizationID)
                    };
                }
                return _filterDescriptors;
            }
        }

        private IEnumerable<int> _downHierarchyOrganizationIDArray;
        public IEnumerable<int> DownHierarchyOrganizationIDArray
        {
            get
            {
                if (_downHierarchyOrganizationIDArray == null)
                {
                    var ds = VMGlobal.SysProcessQuery.DB.ExecuteDataSet("GetOrganizationDownHierarchy", VMGlobal.CurrentUser.OrganizationID);
                    var table = ds.Tables[0];
                    List<int> oids = new List<int>();
                    foreach (DataRow row in table.Rows)
                    {
                        oids.Add((int)row["OrganizationID"]);
                    }
                    _downHierarchyOrganizationIDArray = oids;
                }
                return _downHierarchyOrganizationIDArray;
            }
        }

        #endregion

        public VIPCardVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = new List<VIPCardBO>();
        }

        protected override IEnumerable<VIPCard> SearchData()
        {
            List<VIPCardBO> vips = null;
            if (!FilterConditionHelper.IsConditionSetted(FilterDescriptors, "KindID"))
            {
                vips = base.SearchData().Select(o => new VIPCardBO(o)).ToList();
            }
            else
            {
                var cards = LinqOP.GetDataContext<VIPCard>();
                var maps = LinqOP.GetDataContext<VIPCardKindMapping>();
                var data = from card in cards
                           from map in maps
                           where card.ID == map.CardID
                           select new VIPCardEntityForSearch
                           {
                               Birthday = card.Birthday,
                               Code = card.Code,
                               CustomerName = card.CustomerName,
                               KindID = map.KindID,
                               Sex = card.Sex,
                               MobilePhone = card.MobilePhone,
                               ID = card.ID
                           };
                var filteredData = (IQueryable<VIPCardEntityForSearch>)data.Where(FilterDescriptors);
                var vids = filteredData.Select(o => o.ID).Distinct().ToArray();
                var result = cards.Where(o => vids.Contains(o.ID));
                TotalCount = result.Count();
                vips = result.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).Select(o => new VIPCardBO(o)).ToList();
            }
            VIPCardVM.ApplyVIPKind(vips);
            vips.ForEach(o =>
            {                
                if (!DownHierarchyOrganizationIDArray.Contains(o.OrganizationID))
                {
                    o.MobilePhone = "不可见";
                }
            });
            return vips;
        }

        public override OPResult AddOrUpdate(VIPCard entity)
        {
            VIPCardBO card = (VIPCardBO)entity;
            int id = card.ID;
            var action = new Action(() =>
                {
                    if (id != default(int))
                    {
                        LinqOP.Delete<VIPCardKindMapping>(o => o.CardID == id);
                    }
                    var kindsmap = card.Kinds.Select(o => new VIPCardKindMapping { CardID = card.ID, KindID = o.ID }).ToList();
                    LinqOP.Add<VIPCardKindMapping>(kindsmap);
                });
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var result = base.AddOrUpdate(entity);
                    if (!result.IsSucceed)
                    {
                        return result;
                    }
                    else
                    {
                        action();
                    }
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "保存成功!" };
                }
                catch (Exception e)
                {
                    card.ID = default(int);
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
        }

        public override OPResult Delete(VIPCard vip)
        {
            if (!DownHierarchyOrganizationIDArray.Contains(vip.OrganizationID))
            {
                return new OPResult { IsSucceed = false, Message = "只能删除本级或下级机构创建的VIP信息." };
            }
            if (LinqOP.Any<BillRetail>(o => o.VIPID == vip.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该VIP存在消费记录,不可删除." };
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Delete<VIPCard>(c => c.ID == vip.ID);
                    LinqOP.Delete<VIPCardKindMapping>(c => c.CardID == vip.ID);
                    LinqOP.Delete<VIPPointTrack>(c => c.VIPID == vip.ID);
                    LinqOP.Delete<VIPPredepositTrack>(c => c.VIPID == vip.ID);
                    scope.Complete();                    
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
                }
            }
            return new OPResult { IsSucceed = true, Message = "删除成功!" };
        }

        internal static void ApplyVIPKind(List<VIPCardBO> vips)
        {
            var ids = vips.Select(o => o.ID).ToArray();
            var vks = VMGlobal.DistributionQuery.LinqOP.Search<VIPCardKindMapping>(o => ids.Contains(o.CardID)).ToList();
            var kids = vks.Select(o => o.KindID).Distinct().ToArray();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var ks = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(o => kids.Contains(o.ID) && brandIDs.Contains(o.BrandID)).ToList();
            vips.ForEach(o =>
            {
                var data = from vk in vks
                           from k in ks
                           where vk.KindID == k.ID && vk.CardID == o.ID
                           select k;
                o.Kinds = data.ToList();
            });
        }

        public static OPResult SetPrestorePassword(VIPCard vip, string password)
        {
            vip.PrestorePassword = password.ToMD5String();
            try
            {
                VMGlobal.DistributionQuery.LinqOP.Update<VIPCard>(vip);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "密码设置失败,失败原因:\n" + e.Message };
            }
            return new OPResult { IsSucceed = true, Message = "密码设置成功,请牢记." };
        }
    }
}
