//*********************************************
// 公司名称：              
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-8-27 10:24:23
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

using DistributionViewModel;

namespace DistributionView.Bill
{
    public class DeliveryRowStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ProductForDelivery)
            {
                ProductForDelivery p = item as ProductForDelivery;
                if (p.Quantity > p.OrderQuantity)
                {
                    return DeliveryOverflowOrderStyle;
                }
            }
            return base.SelectStyle(item, container);
        }

        /// <summary>
        /// 发货数量超出订单数量
        /// </summary>
        public Style DeliveryOverflowOrderStyle { get; set; }
    }

    public class StoringWhenReceiveRowStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ProductForStoringWhenReceiving)
            {
                ProductForStoringWhenReceiving p = item as ProductForStoringWhenReceiving;
                if (p.Quantity > p.ReceiveQuantity)
                    return DeliveryOverflowReceiveStyle;
                else if (p.Quantity < p.ReceiveQuantity)
                    return ReceiveOverflowDeliveryStyle;
            }
            return base.SelectStyle(item, container);
        }

        /// <summary>
        /// 收货数量少于发货数量
        /// </summary>
        public Style DeliveryOverflowReceiveStyle { get; set; }
        /// <summary>
        /// 收货数量超出发货数量
        /// </summary>
        public Style ReceiveOverflowDeliveryStyle { get; set; }
    }

    public class StockUpdateRowStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is StocktakeAggregationEntityForStockUpdate)
            {
                StocktakeAggregationEntityForStockUpdate p = item as StocktakeAggregationEntityForStockUpdate;
                if (p.Quantity != p.StockQuantity)
                {
                    return NotEqualStyle;
                }
            }
            return base.SelectStyle(item, container);
        }

        /// <summary>
        /// 盘点数量与库存数量不相等
        /// </summary>
        public Style NotEqualStyle { get; set; }
    }

    public class AllocateRowStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is AllocateEntity)
            {
                AllocateEntity p = item as AllocateEntity;
                if (p.AllocateQuantity > p.AvailableQuantity || p.AllocateQuantity < 0)
                {
                    return AllocateOverflowStockStyle;
                }
            }
            return base.SelectStyle(item, container);
        }

        public Style AllocateOverflowStockStyle { get; set; }
    }
}
