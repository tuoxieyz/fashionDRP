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
    public class StoringAggregationVM : BillReportWithHorSizeVM<ProductShow>
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
                new ItemPropertyDefinition { DisplayName = "入库仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "入库类型", PropertyName = "BillType", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "入库品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
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
            var storingContext = lp.GetDataContext<BillStoring>().Where(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var storingDetailsContext = lp.GetDataContext<BillStoringDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            //var storageContext = lp.GetDataContext<Storage>();
            var data = from storing in storingContext
                       from storingDetails in storingDetailsContext
                       where storing.ID == storingDetails.BillID
                       from product in productContext
                       where product.ProductID == storingDetails.ProductID //&& brandIDs.Contains(product.BrandID)
                       //from storage in storageContext
                       //where storing.StorageID == storage.ID
                       select new StoreOIEntityForAggregation
                       {
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           StorageID = storing.StorageID,
                           CreateTime = storing.CreateTime.Date,
                           BillType = storing.BillType,
                           //ProductCode = product.ProductCode,
                           //BrandCode = product.BrandCode,
                           StyleCode = product.StyleCode,
                           //ColorCode = product.ColorCode,
                           //SizeName = product.SizeName,
                           //Price = product.Price,
                           NameID = product.NameID,
                           Quantity = storingDetails.Quantity
                       };
            data = (IQueryable<StoreOIEntityForAggregation>)data.Where(FilterDescriptors);
            return ReportDataContext.AggregateBill(data);
        }
    }
}
