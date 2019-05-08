using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillSelfCannibalizeSearchVM : BillPagedReportVM<CannibalizeSearchEntity>
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "调拨品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "调出机构", PropertyName = "OrganizationID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "调入机构", PropertyName = "ToOrganizationID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "调拨方向", PropertyName = "Direction", PropertyType = typeof(bool) },
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(bool) },
                        new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) }          
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("CreateDate", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateDate", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Direction", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        /// <summary>
        /// 查询本级调拨单
        /// </summary>
        protected override IEnumerable<CannibalizeSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var cannibalizeContext = lp.Search<BillCannibalize>(o => (o.OrganizationID == VMGlobal.CurrentUser.OrganizationID || o.ToOrganizationID == VMGlobal.CurrentUser.OrganizationID) && brandIDs.Contains(o.BrandID));
            var userContext = lp.GetDataContext<ViewUser>();
            var storageContext = lp.GetDataContext<Storage>();

            var billData = from d in cannibalizeContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           from storage in storageContext
                           where d.StorageID == storage.ID
                           select new CannibalizeSearchEntity
                           {
                               OrganizationID = d.OrganizationID,
                               ToOrganizationID = d.ToOrganizationID,//跟查询条件相关的属性需要显式声明[赋值]，即使父类里已经定义，否则在生成SQL语句的过程中会报错
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime,
                               CreateDate = d.CreateTime.Date,
                               CreatorName = user.Name,
                               StorageID = d.StorageID,
                               StorageName = storage.Name,
                               Status = d.Status,
                               Direction = d.OrganizationID == VMGlobal.CurrentUser.OrganizationID
                           };
            var detailsContext = lp.GetDataContext<BillCannibalizeDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<CannibalizeSearchEntity>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            var cannibalizes = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = cannibalizes.Select(o => o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            var oIDs = cannibalizes.Select(o => o.OrganizationID).Concat(cannibalizes.Select(o => o.ToOrganizationID)).Distinct();
            var organizations = lp.Search<ViewOrganization>(o => oIDs.Contains(o.ID)).ToList();
            cannibalizes.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(o => d.BrandID == o.ID).Name;
                var details = sum.Find(o => o.BillID == d.ID);
                d.Quantity = details.Quantity;
                d.OrganizationName = organizations.Find(o => o.ID == d.OrganizationID).Name;
                d.ToOrganizationName = organizations.Find(o => o.ID == d.ToOrganizationID).Name;
            });
            return cannibalizes;
        }
    }
}
