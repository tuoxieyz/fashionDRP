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
    public class SelfOrderAggregationVM : CommonViewModel<OrderAggregationEntity>//BillReportWithHorSizeVM<OrderAggregationEntity>
    {
        //protected override IEnumerable<string> PropertyNamesForSum
        //{
        //    get
        //    {
        //        return new string[] { "Quantity", "QuaDelivered", "QuaStock" };
        //    }
        //}

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public virtual IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
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
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int) }
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
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        IEnumerable<ItemPropertyDefinition> _detailsPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> DetailsPropertyDefinitions
        {
            get
            {
                if (_detailsPropertyDefinitions == null)
                {
                    _detailsPropertyDefinitions = new List<ItemPropertyDefinition>() {
                        new ItemPropertyDefinition { DisplayName = "仓库", PropertyName = "StorageID", PropertyType = typeof(int) }
                     };
                }
                return _detailsPropertyDefinitions;
            }
        }

        ObservableCollection<FilterDescriptor> _detailsDescriptors;
        public ObservableCollection<FilterDescriptor> DetailsDescriptors
        {
            get
            {
                if (_detailsDescriptors == null)
                {
                    _detailsDescriptors = new ObservableCollection<FilterDescriptor>() 
                    {
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _detailsDescriptors;
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

        private bool _isShowStock = false;
        public bool IsShowStock
        {
            get { return _isShowStock; }
            set
            {
                _isShowStock = value;                
                if (_isShowStock && Entities != null)
                {
                    ShowStock(Entities);
                }
                OnPropertyChanged("IsShowStock");
            }
        }

        protected override IEnumerable<OrderAggregationEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var orderContext = lp.GetDataContext<BillOrder>();
            var orderDetailsContext = lp.GetDataContext<BillOrderDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(o => o.ID);
            var data = from order in orderContext
                       from orderDetails in orderDetailsContext
                       where order.ID == orderDetails.BillID && order.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(order.BrandID) && order.IsDeleted == false
                       from product in productContext
                       where product.ProductID == orderDetails.ProductID
                       select new OrderEntityForAggregation
                       {
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = order.CreateTime.Date,
                           NameID = product.NameID,
                           //ProductCode = product.ProductCode,
                           //BrandCode = product.BrandCode,
                           StyleCode = product.StyleCode,
                           //ColorCode = product.ColorCode,
                           //SizeName = product.SizeName,
                           //Price = product.Price,
                           Quantity = orderDetails.Quantity - orderDetails.QuaCancel,
                           QuaDelivered = orderDetails.QuaDelivered
                       };
            data = (IQueryable<OrderEntityForAggregation>)data.Where(FilterDescriptors);
            var result = this.AggregateOrder(data);
            if (IsShowStock)
            {
                ShowStock(result);
            }
            return result;
        }

        /// <summary>
        /// 订单汇总
        /// </summary>
        /// <param name="showZeroRemainOrder">是否显示剩余订单量为0的数据</param>
        protected virtual ObservableCollection<OrderAggregationEntity> AggregateOrder(IQueryable<OrderEntityForAggregation> data)
        {            
            var tempData = data.GroupBy(o => o.ProductID).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity), QuaDelivered = g.Sum(o => o.QuaDelivered) });
            if (!IsShowZeroRemain)
                tempData = tempData.Where(o => o.Quantity != o.QuaDelivered);
            var temp = tempData.ToList();
            var pids = temp.Select(o => o.Key).ToArray();
            var products = VMGlobal.DistributionQuery.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            temp.RemoveAll(o => o.Quantity == 0 && o.QuaDelivered == 0);
            return new ObservableCollection<OrderAggregationEntity>(temp.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key);
                var entity = new OrderAggregationEntity
                {
                    ProductID = o.Key,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    NameID = product.NameID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, product.BYQID, product.Price),
                    Quantity = o.Quantity,
                    QuaDelivered = o.QuaDelivered
                };
                entity.ColorCode = VMGlobal.Colors.Find(c => c.ID == entity.ColorID).Code;
                entity.SizeName = VMGlobal.Sizes.Find(c => c.ID == entity.SizeID).Name;
                entity.ProductName = VMGlobal.ProNames.Find(c => c.ID == entity.NameID).Name;
                var byq = VMGlobal.BYQs.Find(c => c.ID == entity.BYQID);
                entity.BrandID = byq.BrandID;
                entity.BrandCode = VMGlobal.PoweredBrands.Find(c => c.ID == entity.BrandID).Code;
                return entity;
            }).ToList());
        }

        /// <summary>
        /// 显示当前库存
        /// </summary>
        protected virtual void ShowStock(IEnumerable<OrderAggregationEntity> data)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var pids = data.Select(o => o.ProductID).ToArray();
            var sids = StorageInfoVM.Storages.Select(o => o.ID);
            IQueryable<Stock> stockContext = lp.Search<Stock>(o => sids.Contains(o.StorageID) && pids.Contains(o.ProductID));
            stockContext = ((IQueryable<Stock>)lp.Search<Stock>().Where(DetailsDescriptors));
            var stocks = this.SearchStock(stockContext);
            foreach (var d in data)
            {
                var s = stocks.Find(o => o.ProductID == d.ProductID);
                if (s == null)
                    d.QuaStock = 0;
                else
                    d.QuaStock = s.Quantity;
            }
        }

        /// <summary>
        /// 库存查询
        /// </summary>
        /// <param name="productIDs">待查询的成品ID集合</param>
        /// <param name="storageFilter">仓库筛选条件</param>
        protected List<ProductQuantity> SearchStock(IQueryable<Stock> stockContext)
        {
            //if (!FilterConditionHelper.IsConditionSetted(storageFilter, "StorageID"))
            //{
            //    var sids = lp.Search<Storage, int>(o => o.ID, o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Flag).ToArray();
            //    stock = lp.Search<Stock>(o => sids.Contains(o.StorageID) && productIDs.Contains(o.ProductID));
            //}
            //else
            //{
            //    stock = ((IQueryable<Stock>)lp.Search<Stock>().Where(storageFilter)).Where(o => productIDs.Contains(o.ProductID));
            //}
            var temp = stockContext.GroupBy(o => o.ProductID).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            return temp.Select(o => new ProductQuantity
            {
                ProductID = o.Key,
                Quantity = o.Quantity
            }).ToList();
        }

        private void UnShowZeroRemain()
        {
            var data = Entities as ObservableCollection<OrderAggregationEntity>;
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var d = data[i];
                    if (d.QuaDelivered == d.Quantity)
                    {
                        data.Remove(d);
                        i--;
                    }
                }
            }
        }
    }
}
