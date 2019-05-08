using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    /// <summary>
    /// 入库类型
    /// 和单据类型BillTypeEnum一一对应
    /// </summary>
    public enum BillStoringTypeEnum
    {
        成品入库 = 0,//入库单
        上级发货入库 = 3,//发货单
        移库入库 = 7,//移库单
        零售入库 = 8,//零售单
        调拨入库 = 9,
        下级退货入库 = 12,
        交接入库 = 13
        //发货装箱之拆箱返仓 = 99
    }
}
