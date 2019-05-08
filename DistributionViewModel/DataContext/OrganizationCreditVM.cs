using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using SysProcessModel;
using Kernel;
using ERPViewModelBasic;
using SysProcessViewModel;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class OrganizationCreditVM : PagedEditSynchronousVM<OrganizationCredit>
    {
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
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}, 
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "有效期", PropertyName = "EndDate", PropertyType = typeof(DateTime)}
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
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false)
                    };
                }
                return _filterDescriptors;
            }
        }

        public SysOrganizationBO CurrentOrganization { get; private set; }

        #endregion

        public OrganizationCreditVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = new List<OrganizationCredit>();
        }

        protected override IEnumerable<OrganizationCredit> SearchData()
        {
            var oids = VMGlobal.ChildOrganizations.Select(o => o.ID);
            var all = LinqOP.Search<OrganizationCredit>(o => oids.Contains(o.OrganizationID));
            var filteredData = (IQueryable<OrganizationCredit>)all.Where(FilterDescriptors);
            TotalCount = filteredData.Count();
            return filteredData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }

        /// <summary>
        /// 查看机构特定品牌的资信是否已设置过
        /// </summary>
        private bool CreditIsExist(OrganizationCredit credit)
        {
            if (credit.ID == default(int))
            {
                return LinqOP.Any<OrganizationCredit>(oc => oc.OrganizationID == credit.OrganizationID && oc.BrandID == credit.BrandID);
            }
            else
            {
                return LinqOP.Any<OrganizationCredit>(oc => oc.ID != credit.ID && oc.OrganizationID == credit.OrganizationID && oc.BrandID == credit.BrandID);
            }
        }

        public override OPResult AddOrUpdate(OrganizationCredit entity)
        {
            if (CreditIsExist(entity))
            {
                return new OPResult { IsSucceed = false, Message = "该机构已经设置了该品牌的资信额度." };
            }
            return base.AddOrUpdate(entity);
        }

        public OPResult RaiseCredit(OrganizationCredit credit, int increase)
        {
            credit.CreditMoney += increase;
            var result = base.AddOrUpdate(credit);
            if (result.IsSucceed)
                credit.OnPropertyChanged("CreditMoney");
            else
                credit.CreditMoney -= increase;
            return result;
        }
    }
}
