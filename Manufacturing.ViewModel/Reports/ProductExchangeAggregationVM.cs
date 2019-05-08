using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using ManufacturingModel;
using DistributionModel;
using SysProcessModel;
using ViewModelBasic;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class ProductExchangeAggregationVM : BillReportWithHorSizeVM<ProductForProductExchange>
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
                        new ItemPropertyDefinition { DisplayName = "交接品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "交接状态", PropertyName = "Status", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) },
                        new ItemPropertyDefinition { DisplayName = "工厂", PropertyName = "OuterFactoryID", PropertyType = typeof(int) }
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<ProductForProductExchange> SearchData()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var productExchangeContext = lp.GetDataContext<BillProductExchange>();
            var detailsContext = lp.GetDataContext<BillProductExchangeDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            productExchangeContext = productExchangeContext.Where(o => brandIDs.Contains(o.BrandID) && !o.IsDeleted);
            var data = from pe in productExchangeContext
                       from details in detailsContext
                       where pe.ID == details.BillID //&& pe.IsDeleted == false
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new ProductForProductExchange
                       {
                           BrandID = product.BrandID,
                           ProductID = product.ProductID,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Quantity = details.Quantity,
                           ProductCode = product.ProductCode,
                           CreateDate = pe.CreateTime.Date,
                           OuterFactoryID = pe.OuterFactoryID
                       };
            data = (IQueryable<ProductForProductExchange>)data.Where(FilterDescriptors);
            var result = data.GroupBy(o => new { o.ProductCode, o.StyleCode, o.ColorID, o.SizeID, o.BrandID }).Select(g => new ProductForProductExchange
            {
                ProductCode = g.Key.ProductCode,
                BrandID = g.Key.BrandID,
                StyleCode = g.Key.StyleCode,
                ColorID = g.Key.ColorID,
                SizeID = g.Key.SizeID,
                Quantity = g.Sum(o => o.Quantity)
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
            }
            return result;
        }
    }
}
