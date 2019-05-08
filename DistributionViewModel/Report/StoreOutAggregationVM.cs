using DistributionModel;
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
    public class StoreOutAggregationVM : BillReportWithHorSizeVM<ProductShow>
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},                
                new ItemPropertyDefinition { DisplayName = "出库仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "出库品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                new ItemPropertyDefinition { DisplayName = "出库类型", PropertyName = "BillType", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) },
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
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BillType", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<ProductShow> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var storeoutContext = lp.GetDataContext<BillStoreOut>().Where(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var detailsContext = lp.GetDataContext<BillStoreOutDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from storeout in storeoutContext
                       from details in detailsContext
                       where storeout.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new StoreOIEntityForAggregation
                       {
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           StorageID = storeout.StorageID,
                           CreateTime = storeout.CreateTime.Date,
                           BillType = storeout.BillType,
                           StyleCode = product.StyleCode,
                           NameID = product.NameID,
                           Quantity = details.Quantity
                       };
            data = (IQueryable<StoreOIEntityForAggregation>)data.Where(FilterDescriptors);
            return ReportDataContext.AggregateBill(data);
        }
    }
}
