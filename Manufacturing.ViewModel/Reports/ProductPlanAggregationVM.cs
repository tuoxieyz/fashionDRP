using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using SysProcessViewModel;
using ManufacturingModel;
using SysProcessModel;
using System.Collections.ObjectModel;

namespace Manufacturing.ViewModel
{
    public class ProductPlanAggregationVM : CommonViewModel<ProductForProduceBrush>
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
                        new ItemPropertyDefinition { DisplayName = "生产品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) },
                        new ItemPropertyDefinition { DisplayName = "交货日期", PropertyName = "DeliveryDate", PropertyType = typeof(DateTime) }
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
                        new FilterDescriptor("CreateDate", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("CreateDate", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        private bool _isShowZeroRemain = false;
        public bool IsShowZeroRemain
        {
            get { return _isShowZeroRemain; }
            set
            {
                _isShowZeroRemain = value;
                if (!_isShowZeroRemain)
                {
                    UnShowZeroRemain();
                }
                else if (Entities != null)
                {
                    Entities = this.SearchData();
                }
            }
        }

        protected override IEnumerable<ProductForProduceBrush> SearchData()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var planContext = lp.GetDataContext<BillProductPlan>();
            var factoryContext = lp.GetDataContext<Factory>();
            var detailsContext = lp.GetDataContext<BillProductPlanDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            planContext = planContext.Where(o => brandIDs.Contains(o.BrandID));
            var data = from plan in planContext
                       from details in detailsContext
                       where plan.ID == details.BillID //&& plan.IsDeleted == false
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new ProductForProduceBrush
                       {
                           BrandID = product.BrandID,
                           ProductID = product.ProductID,
                           DeliveryDate = details.DeliveryDate,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Quantity = details.Quantity - details.QuaCancel,
                           QuaCompleted = details.QuaCompleted,
                           ProductCode = product.ProductCode,
                           CreateDate = plan.CreateTime.Date
                       };
            data = (IQueryable<ProductForProduceBrush>)data.Where(FilterDescriptors);
            if (!IsShowZeroRemain)
                data = data.Where(o => o.Quantity != o.QuaCompleted);
            var result = data.GroupBy(o => new { o.ProductCode, o.StyleCode, o.ColorID, o.SizeID, o.BrandID }).Select(g => new ProductForProduceBrush
            {
                ProductCode = g.Key.ProductCode,
                BrandID = g.Key.BrandID,
                StyleCode = g.Key.StyleCode,
                ColorID = g.Key.ColorID,
                SizeID = g.Key.SizeID,
                Quantity = g.Sum(o => o.Quantity),
                QuaCompleted = g.Sum(o => o.QuaCompleted)
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
            }
            return new ObservableCollection<ProductForProduceBrush>(result);
        }

        private void UnShowZeroRemain()
        {
            var data = Entities as ObservableCollection<ProductForProduceBrush>;
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var d = data[i];
                    if (d.QuaCancel + d.QuaCompleted == d.Quantity)
                    {
                        data.Remove(d);
                        i--;
                    }
                }
            }
        }
    }
}
