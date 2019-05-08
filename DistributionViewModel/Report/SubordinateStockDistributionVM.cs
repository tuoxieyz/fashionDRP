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
using ViewModelBasic;

namespace DistributionViewModel
{
    public class SubordinateStockDistributionVM : CommonViewModel<DistributionEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("NameID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<DistributionEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var storageContext = lp.Search<Storage>(o => oids.Contains(o.OrganizationID));
            var stockContext = lp.GetDataContext<Stock>();
            var productContext = lp.Search<ViewProduct>(o => brandIDs.Contains(o.BrandID));
            var orgContext = lp.GetDataContext<ViewOrganization>();

            var data = from stock in stockContext
                       from storage in storageContext
                       where stock.StorageID == storage.ID && stock.Quantity != 0
                       from org in orgContext
                       where storage.OrganizationID == org.ID
                       from product in productContext
                       where product.ProductID == stock.ProductID
                       select new BillEntityForAggregation
                       {
                           BrandID = product.BrandID,
                           ProductID = product.ProductID,
                           StyleCode = product.StyleCode,
                           Quantity = stock.Quantity,
                           OrganizationID = storage.OrganizationID,
                           NameID = product.NameID
                       };
            var filtedData = (IQueryable<BillEntityForAggregation>)data.Where(FilterDescriptors);
            var sum = filtedData.GroupBy(o => new { o.ProductID, o.OrganizationID }).Select(g => new
            {
                Key = g.Key,
                Quantity = g.Sum(o => o.Quantity)
            }).ToList();
            var pids = sum.Select(o => o.Key.ProductID).ToArray();
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = sum.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key.ProductID);
                return new DistributionEntity
                {
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    OrganizationID = o.Key.OrganizationID,
                    OrganizationName = OrganizationArray.First(c => c.ID == o.Key.OrganizationID).Name
                };
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                r.BrandID = byq.BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
            }
            return result;
        }

        public IEnumerable<DistributionEntity> Search()
        {
            return this.SearchData();
        }
    }
}
