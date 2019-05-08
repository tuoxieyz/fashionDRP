using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Kernel;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class MonthSaleTagetVM : PagedEditSynchronousVM<RetailMonthTaget>
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
                        new ItemPropertyDefinition { DisplayName = "年", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "月", PropertyName = "Month", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "零售机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("Year", FilterOperator.IsEqualTo,DateTime.Now.Year)
                        //new FilterDescriptor("Month", FilterOperator.IsEqualTo,DateTime.Now.Month)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public MonthSaleTagetVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = new List<RetailMonthTagetBO>();
        }

        protected override IEnumerable<RetailMonthTaget> SearchData()
        {
            var oids = OrganizationListVM.CurrentAndChildrenOrganizations.Select(o=>o.ID).ToArray();
            return LinqOP.Search<RetailMonthTaget>(o => oids.Contains(o.OrganizationID)).Select(o => new RetailMonthTagetBO(o)).ToList();
        }

        public override OPResult AddOrUpdate(RetailMonthTaget target)
        {
            if (target.ID == default(int))
            {
                if (LinqOP.Any<RetailMonthTaget>(o => o.OrganizationID == target.OrganizationID && o.Year == target.Year && o.Month == target.Month))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构设置了该月指标." };
                }
            }
            else if (LinqOP.Any<RetailMonthTaget>(o => o.OrganizationID == target.OrganizationID && o.ID != target.ID && o.Year == target.Year && o.Month == target.Month))
            {
                return new OPResult { IsSucceed = false, Message = "已为该机构设置了该月指标." };
            }
            return base.AddOrUpdate(target);
        }

        public override OPResult Delete(RetailMonthTaget target)
        {
            if (target.OrganizationID == VMGlobal.CurrentUser.OrganizationID)
            {
                return new OPResult { IsSucceed = false, Message = "不能修改本机构自身的月度指标" };
            }
            return base.Delete(target);
        }
    }
}
