using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class OtherShopStockStatisticsVM : CommonViewModel<DistributionEntity>
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
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue),
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<DistributionEntity> SearchData()
        {
            throw new NotImplementedException();
        }
    }
}
