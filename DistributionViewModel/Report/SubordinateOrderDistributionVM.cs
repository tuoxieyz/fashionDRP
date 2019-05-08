using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class SubordinateOrderDistributionVM
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "订货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        public List<OrderDistributionEntity> GetSubordinateOrderDistribution()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var data = ReportDataContext.GetSubordinateOrderAggregation(FilterDescriptors, oids);
            var temp = data.GroupBy(o => new { o.ProductID, o.OrganizationID }).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity), QuaDelivered = g.Sum(o => o.QuaDelivered) }).ToList();
            var pids = temp.Select(o => o.Key.ProductID).ToArray();
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            //var oids = temp.Select(o => o.Key.OrganizationID).ToArray();
            //var organizations = lp.Search<ViewOrganization>(o => oids.Contains(o.ID)).ToList();
            temp.RemoveAll(o => o.Quantity == 0 && o.QuaDelivered == 0);
            var result = temp.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key.ProductID);
                var organization = OrganizationArray.First(p => p.ID == o.Key.OrganizationID);
                return new OrderDistributionEntity
                {
                    OrganizationID = o.Key.OrganizationID,
                    OrganizationName = organization.Name,
                    ProductID = o.Key.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    QuaDelivered = o.QuaDelivered
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
}
