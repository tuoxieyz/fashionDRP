using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    /// <summary>
    /// 出库类型
    /// 和单据类型BillTypeEnum一一对应
    /// </summary>
    public enum BillStoreOutTypeEnum
    {
        发货出库 = 3,//发货单
        移库出库 = 7,//移库单
        零售出库 = 8,//零售单
        调拨出库 = 9,
        退货出库 = 12
    }
}
