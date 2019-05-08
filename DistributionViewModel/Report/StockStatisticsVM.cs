using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using DomainLogicEncap;
using ViewModelBasic;
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class StockStatisticsVM : BillReportWithHorSizeVM<StockStatisticsEntity>
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public virtual IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
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
                        new ItemPropertyDefinition { DisplayName = "尺码", PropertyName = "SizeID", PropertyType = typeof(int)}, 
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
        public virtual CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
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
            return this.SearchData(VMGlobal.CurrentUser.OrganizationID);
        }

        protected IEnumerable<StockStatisticsEntity> SearchData(int organizationID)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var provider = VMGlobal.DistributionQuery.QueryProvider;
            var stockContext = VMGlobal.DistributionQuery.LinqOP.GetDataContext<Stock>();
            var storageContext = VMGlobal.DistributionQuery.LinqOP.Search<Storage>(o => o.OrganizationID == organizationID && o.Flag);
            var productContext = provider.GetTable<Product>("SysProcess.dbo.Product");
            var styleContext = provider.GetTable<ProStyle>("SysProcess.dbo.ProStyle");
            var pnameContext = provider.GetTable<ProName>("SysProcess.dbo.ProName");
            var brandContext = provider.GetTable<ProBrand>("SysProcess.dbo.ProBrand");
            var colorContext = provider.GetTable<ProColor>("SysProcess.dbo.ProColor");
            var sizeContext = provider.GetTable<ProSize>("SysProcess.dbo.ProSize");
            var byqContext = provider.GetTable<ProBYQ>("SysProcess.dbo.ProBYQ");

            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);

            var data = from stock in stockContext
                       from storage in storageContext
                       where stock.StorageID == storage.ID && stock.Quantity != 0
                       from product in productContext
                       where stock.ProductID == product.ID
                       from style in styleContext
                       where product.StyleID == style.ID
                       from byq in byqContext
                       where style.BYQID == byq.ID
                       from brand in brandContext
                       where byq.BrandID == brand.ID && brandIDs.Contains(brand.ID)
                       from color in colorContext
                       where product.ColorID == color.ID
                       from size in sizeContext
                       where product.SizeID == size.ID
                       from pname in pnameContext
                       where style.NameID == pname.ID
                       select new StockStatisticsEntity
                       {
                           StorageID = storage.ID,
                           NameID = pname.ID,
                           StyleCode = style.Code,
                           ColorCode = color.Code,
                           ColorName = color.Name,
                           SizeID = size.ID,
                           BrandID = brand.ID,
                           Year = byq.Year,
                           Quarter = byq.Quarter,
                           ProductCode = product.Code,
                           //StorageName = storage.Name,
                           BrandCode = brand.Code,
                           SizeName = size.Name,
                           ProductName = pname.Name,
                           Quantity = stock.Quantity,
                           Price = style.Price,
                           ProductID = product.ID,
                           BYQID = byq.ID
                       };
            var temp = ((IQueryable<StockStatisticsEntity>)data.Where(FilterDescriptors)).ToList();//即使filters中有data没有的过滤属性，也不会出错，但是会产生0<>0的恒为假条件
            var groups = temp.GroupBy(o => o.ProductID).Select(g => new { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            var result = groups.Select(o =>
            {
                var item = temp.Find(t => t.ProductID == o.ProductID);
                item.Quantity = o.Quantity;
                item.Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, item.BYQID, item.Price);
                item.QuarterName = VMGlobal.Quarters.Find(q => q.ID == item.Quarter).Name;
                return item;
            }).ToList();
            return result;
        }

    }

    public class StockStatisticsEntity : DistributionProductShow
    {
        public int StorageID { get; set; }
        public int SizeID { get; set; }
        public string StorageName { get; set; }
        public string QuarterName { get; set; }
    }
}
