using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class PreStoreSearchVM : CommonViewModel<PreStoreSearchEntity>
    {
        private List<DataState> _kinds;

        public List<DataState> Kinds
        {
            get
            {
                if (_kinds == null)
                {
                    _kinds = new List<DataState>();
                    _kinds.Add(new DataState { Flag = true, Name = "充值" });
                    _kinds.Add(new DataState { Flag = false, Name = "消费" });
                }
                return _kinds;
            }
        }

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "发生日期", PropertyName = "OccurDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "VIP卡号", PropertyName = "VIPCode", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "发生类型", PropertyName = "Kind", PropertyType = typeof(bool)}
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
                        new FilterDescriptor("OccurDate", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("OccurDate", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("Kind", FilterOperator.IsEqualTo, true)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<PreStoreSearchEntity> SearchData()
        {
            var oids = OrganizationListVM.GetOrganizationDownHierarchy(VMGlobal.CurrentUser.OrganizationID);
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var vips = lp.GetDataContext<VIPCard>();
            var prestores = lp.Search<VIPPredepositTrack>(o => oids.Contains(o.OrganizationID));
            var organizations = lp.GetDataContext<ViewOrganization>();
            var query = from prestore in prestores
                        from vip in vips
                        where prestore.VIPID == vip.ID
                        from organization in organizations
                        where vip.OrganizationID == organization.ID
                        select new PreStoreSearchEntity
                        {
                            ConsumeMoney = prestore.ConsumeMoney,
                            CreateTime = prestore.CreateTime,
                            OccurDate = prestore.CreateTime.Date,
                            FreeMoney = prestore.FreeMoney,
                            Kind = prestore.Kind,
                            OrganizationName = organization.Name,
                            RefrenceBillCode = prestore.RefrenceBillCode,
                            Remark = prestore.Remark,
                            StoreMoney = prestore.StoreMoney,
                            VIPCode = vip.Code,
                            VIPName = vip.CustomerName
                        };
            var filtedData = (IQueryable<PreStoreSearchEntity>)query.Where(FilterDescriptors);
            var result = filtedData.ToList();
            foreach (var r in result)
            {
                r.KindName = Kinds.Find(o => o.Flag == r.Kind).Name;
            }
            return result;
        }
    }

    public class PreStoreSearchEntity
    {
        public string VIPCode { get; set; }
        public string VIPName { get; set; }
        public string OrganizationName { get; set; }
        public string RefrenceBillCode { get; set; }
        public bool Kind { get; set; }
        public string KindName { get; set; }
        public decimal StoreMoney { get; set; }
        public decimal FreeMoney { get; set; }
        public decimal ConsumeMoney { get; set; }
        public string Remark { get; set; }
        public DateTime OccurDate { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
