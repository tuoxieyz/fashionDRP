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
    public class SelfCannibalizeAggregationVM : BillReportWithHorSizeVM<CannibalizeAggregationEntity>
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
                        new ItemPropertyDefinition { DisplayName = "调拨品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "调出机构", PropertyName = "OrganizationID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "调入机构", PropertyName = "ToOrganizationID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "调拨方向", PropertyName = "Direction", PropertyType = typeof(bool) },
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(bool) },
                        new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) }
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Direction", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<CannibalizeAggregationEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var cannibalizeContext = lp.Search<BillCannibalize>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID || o.ToOrganizationID == VMGlobal.CurrentUser.OrganizationID);
            var detailsContext = lp.GetDataContext<BillCannibalizeDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var data = from cannibalize in cannibalizeContext
                       from details in detailsContext
                       where cannibalize.ID == details.BillID && brandIDs.Contains(cannibalize.BrandID)
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new CannibalizeEntityForAggregation
                       {
                           OrganizationID = cannibalize.OrganizationID,
                           ToOrganizationID = cannibalize.ToOrganizationID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = cannibalize.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           Status = cannibalize.Status,
                           Direction = cannibalize.OrganizationID == VMGlobal.CurrentUser.OrganizationID
                       };

            data = (IQueryable<CannibalizeEntityForAggregation>)data.Where(FilterDescriptors);

            var temp = data.GroupBy(o => o.ProductID).Select(g => new { g.Key, OutQuantity = g.Sum(o => o.Direction ? o.Quantity : 0), InQuantity = g.Sum(o => o.Direction ? 0 : o.Quantity) }).ToList();
            var pids = temp.Select(o => o.Key).ToArray();
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = temp.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key);
                return new CannibalizeAggregationEntity
                {
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    OutQuantity = o.OutQuantity,
                    InQuantity = o.InQuantity
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
    }

    public class CannibalizeAggregationEntity : ProductShow
    {
        public int OutQuantity { get; set; }
        public int InQuantity { get; set; }
    }
}
