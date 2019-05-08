using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using System.Windows.Input;
using DomainLogicEncap;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class SubordinateRetailAchievementContrailVM : CommonViewModel<RetailDistributionEntity>
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime) }
                        //new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date)
                    };
                }
                return _filterDescriptors;
            }
        }

        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public int MaxQuantity
        {
            get
            {
                if (Entities != null)
                    return Entities.Max(o => o.Quantity);
                return 0;
            }
        }

        public int MinQuantity
        {
            get
            {
                if (Entities != null)
                {
                    var min = Entities.Min(o => o.Quantity);
                    return min > 0 ? 0 : min;
                }
                return 0;
            }
        }

        public decimal MaxCostMoney
        {
            get
            {
                if (Entities != null)
                    return Entities.Max(o => o.CostMoney);
                return 0;
            }
        }

        public decimal MinCostMoney
        {
            get
            {
                if (Entities != null)
                {
                    var min = Entities.Min(o => o.CostMoney);
                    return min > 0 ? 0 : min;
                }
                return 0;
            }
        }

        /// <summary>
        /// 获取下级零售业绩
        /// </summary>
        protected override IEnumerable<RetailDistributionEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            //var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var retailContext = lp.GetDataContext<BillRetail>();
            //var orgContext = lp.Search<ViewOrganization>(o => oids.Contains(o.ID));

            var data = from retail in retailContext
                       where oids.Contains(retail.OrganizationID)
                       select new RetailEntityForDistribution
                       {
                           CreateTime = retail.CreateTime.Date,
                           Quantity = retail.Quantity,
                           CostMoney = retail.CostMoney,
                           ReceiveMoney = retail.ReceiveMoney,
                           TicketMoney = retail.TicketMoney,
                           OrganizationID = retail.OrganizationID
                       };
            var filtedData = (IQueryable<RetailEntityForDistribution>)data.Where(FilterDescriptors);
            var result = filtedData.GroupBy(o => o.OrganizationID).Select(g => new RetailDistributionEntity
            {
                OrganizationID = g.Key,
                Quantity = g.Sum(o => o.Quantity),
                CostMoney = g.Sum(o => o.CostMoney),
                ReceiveMoney = g.Sum(o => o.ReceiveMoney),
                TicketMoney = g.Sum(o => o.TicketMoney)
            }).ToList();
            result.ForEach(o => o.OrganizationName = OrganizationArray.First(c => c.ID == o.OrganizationID).Name);
            return result;
        }
    }
}
