using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace DistributionModel
{
    //BillDeliveryDetails
    public class BillDeliveryDetails : BillDetailBase
    {
        public decimal Discount { get; set; }
        /// <summary>
        /// 发货时的折后价（未免成品资料价格修改或上浮策略修改导致实时查询时价格差错）
        /// </summary>
        public decimal Price { get; set; }
    }
}

