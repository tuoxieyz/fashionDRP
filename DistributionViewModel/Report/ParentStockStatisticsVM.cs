using DistributionModel;
using DomainLogicEncap;
using ERPViewModelBasic;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;

namespace DistributionViewModel
{
    public class ParentStockStatisticsVM : StockStatisticsVM
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public override IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "色号", PropertyName = "ColorCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)},
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("ColorCode", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false)
                    };
                }
                return _filterDescriptors;
            }
        }

        /// <summary>
        /// 库存统计
        /// </summary>
        protected override IEnumerable<StockStatisticsEntity> SearchData()
        {
            return this.SearchData(OrganizationListVM.CurrentOrganization.ParentID);
        }
    }
}
