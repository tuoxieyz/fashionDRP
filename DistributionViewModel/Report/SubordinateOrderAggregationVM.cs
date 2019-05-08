using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class SubordinateOrderAggregationVM : SelfOrderAggregationVM//CommonViewModel<OrderAggregationEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        //IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        //public override IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        //{
        //    get
        //    {
        //        if (_itemPropertyDefinitions == null)
        //        {
        //            _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
        //            {  
        //                new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
        //                new ItemPropertyDefinition { DisplayName = "订货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
        //                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) }
        //            };
        //        }
        //        return _itemPropertyDefinitions;
        //    }
        //}

        //CompositeFilterDescriptorCollection _filterDescriptors;
        //public override CompositeFilterDescriptorCollection FilterDescriptors
        //{
        //    get
        //    {
        //        if (_filterDescriptors == null)
        //        {
        //            _filterDescriptors = new CompositeFilterDescriptorCollection() 
        //            {  
        //                new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
        //                new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
        //                new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
        //            };
        //        }
        //        return _filterDescriptors;
        //    }
        //}

        protected override IEnumerable<OrderAggregationEntity> SearchData()
        {
            var data = ReportDataContext.GetSubordinateOrderAggregation(FilterDescriptors, OrganizationArray.Select(o => o.ID).ToArray());
            var result = this.AggregateOrder(data);
            if (IsShowStock)
            {
                ShowStock(result);
            }
            return result;
        }

        /// <summary>
        /// 显示当前库存
        /// </summary>
        protected override void ShowStock(IEnumerable<OrderAggregationEntity> data)
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
            stocks = this.GetAvailableStock(stocks);
            foreach (var d in data)
            {
                var s = stocks.Find(o => o.ProductID == d.ProductID);
                if (s == null)
                    d.QuaAvailableStock = 0;
                else
                    d.QuaAvailableStock = s.Quantity;
            }
        }

        /// <summary>
        /// 可用库存
        /// </summary>
        private List<ProductQuantity> GetAvailableStock(List<ProductQuantity> stocks)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var pids = stocks.Select(o => o.ProductID);
            int status = (int)BillDeliveryStatusEnum.已装箱未配送;
            var deliveries = (IQueryable<BillDelivery>)lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Status == status).Where(DetailsDescriptors);
            var deliveryDetails = lp.GetDataContext<BillDeliveryDetails>();
            var deliveryData = from delivery in deliveries
                               from details in deliveryDetails
                               where delivery.ID == details.BillID && pids.Contains(details.ProductID)
                               select new ProductQuantity
                               {
                                   Quantity = details.Quantity,
                                   ProductID = details.ProductID
                               };
            //占用库存
            var deliveryResult = deliveryData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            stocks.ForEach(o =>
            {
                var dtemp = deliveryResult.Find(d => d.ProductID == o.ProductID);
                o.Quantity = o.Quantity - (dtemp == null ? 0 : dtemp.Quantity);
            });
            return stocks;
        }
    }
}
