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
    public class ShopExpenseSetVM : PagedEditSynchronousVM<ShopExpense>
    {
        #region 属性

        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public IEnumerable<ShopExpenseKind> ExpenseKinds { get; private set; }

        List<ItemPropertyDefinition> _itemPropertyDefinitions;
        public List<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "发生日期", PropertyName = "OccurDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "费用类别", PropertyName = "ExpenseKindID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("OccurDate", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("OccurDate", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public ShopExpenseSetVM(): base(VMGlobal.DistributionQuery.LinqOP)
        {
            ExpenseKinds = LinqOP.Search<ShopExpenseKind>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            Entities = new List<ShopExpense>();
        }

        protected override IEnumerable<ShopExpense> SearchData()
        {
            var oids = OrganizationArray.Select(o => o.ID);
            var data = (IQueryable<ShopExpense>)LinqOP.Search<ShopExpense>(o => oids.Contains(o.OrganizationID)).Where(FilterDescriptors);
            TotalCount = data.Count();
            return data.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }
    }
}
