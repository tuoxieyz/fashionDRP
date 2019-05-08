using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    public enum BillTypeEnum
    {
        /// <summary>
        /// 入库单
        /// </summary>
        [EnumDescription("入库单")]
        BillStoring = 0,
        /// <summary>
        /// 订单
        /// </summary>
        [EnumDescription("订货单")]
        BillOrder = 1,
        /// <summary>
        /// 配货单
        /// </summary>    
        [EnumDescription("配货单")]
        BillAllocate = 2,
        /// <summary>
        /// 发货单
        /// </summary>
        [EnumDescription("发货单")]
        BillDelivery = 3,
        /// <summary>
        /// 出库单
        /// </summary>
        [EnumDescription("出库单")]
        BillStoreOut = 4,
        /// <summary>
        /// 盘点单
        /// </summary>
        [EnumDescription("盘点单")]
        BillStocktake = 5,
        /// <summary>
        /// 盈亏单
        /// </summary>
        [EnumDescription("盈亏单")]
        BillStocktakeContrast = 6,
        /// <summary>
        /// 移库单
        /// </summary>
        [EnumDescription("移库单")]
        BillStoreMove = 7,
        /// <summary>
        /// 零售单
        /// </summary>   
        [EnumDescription("零售单")]
        BillRetail = 8,
        /// <summary>
        /// 调拨单
        /// </summary>
        [EnumDescription("调拨单")]
        BillCannibalize = 9,
        /// <summary>
        /// 扣款单
        /// </summary>
        [EnumDescription("扣款单")]
        VoucherDeductMoney = 10,
        /// <summary>
        /// 收款单
        /// </summary>
        [EnumDescription("收款单")]
        VoucherReceiveMoney = 11,
        /// <summary>
        /// 退货单
        /// </summary>
        [EnumDescription("退货单")]
        BillGoodReturn = 12,

        [EnumDescription("生产交接单")]
        BillProductExchange = 13,
        /// <summary>
        /// 生产委托单
        /// </summary>
        [EnumDescription("生产委托单")]
        BillSubcontract = 14,
        [EnumDescription("生产计划单")]
        BillProductPlan = 15
        //[EnumDescription("发货装箱之拆箱返仓")]
        //UnpackageDelivery = 99
    }
}
