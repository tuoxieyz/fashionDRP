using DistributionModel;
using DomainLogicEncap;
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
    public class StockABCAnalysisVM : CommonViewModel<StockStatisticsEntity>
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
                        new ItemPropertyDefinition { DisplayName = "仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "色号", PropertyName = "ColorCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)}
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
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Year", FilterOperator.IsEqualTo,DateTime.Now.Year)
                    };
                }
                return _filterDescriptors;
            }
        }

        public decimal AmountCostMoney { get; set; }
        public int AmountQuantity { get; set; }

        protected override IEnumerable<StockStatisticsEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var provider = VMGlobal.DistributionQuery.QueryProvider;
            var stockContext = VMGlobal.DistributionQuery.LinqOP.GetDataContext<Stock>();
            var storageContext = VMGlobal.DistributionQuery.LinqOP.Search<Storage>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Flag);
            var productContext = lp.GetDataContext<ViewProduct>();
            var byqs = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProBYQ>("SysProcess.dbo.ProBYQ");
            var colors = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProColor>("SysProcess.dbo.ProColor");
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);

            var data = from stock in stockContext
                       from storage in storageContext
                       where stock.StorageID == storage.ID && stock.Quantity != 0
                       from product in productContext
                       where stock.ProductID == product.ProductID && brandIDs.Contains(product.BrandID)
                       from byq in byqs
                       where product.BYQID == byq.ID
                       from color in colors
                       where product.ColorID == color.ID
                       select new StockStatisticsEntity
                       {
                           StorageID = storage.ID,
                           NameID = product.NameID,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           BYQID = product.BYQID,
                           Quantity = stock.Quantity,
                           Price = product.Price,
                           ProductID = product.ProductID,
                           BrandID = byq.BrandID,
                           Year = byq.Year,
                           Quarter = byq.Quarter,
                           ColorCode = color.Code
                       };
            var result = ((IQueryable<StockStatisticsEntity>)data.Where(FilterDescriptors)).ToList();
            foreach (var r in result)
            {
                r.ProductName = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
            }
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            result.ForEach(o => o.Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.BYQID, o.Price));
            AmountCostMoney = result.Sum(o => o.Price * o.Quantity);
            AmountQuantity = result.Sum(o => o.Quantity);
            OnPropertyChanged("AmountCostMoney");
            OnPropertyChanged("AmountQuantity");
            return result;
        }
    }
}
