using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SysProcessViewModel
{
    /// <summary>
    /// 即时消息接收权限
    /// </summary>
    [Flags]
    public enum IMReceiveAccessEnum
    {
        //0为无权限

        /// <summary>
        /// 机构资信变动消息
        /// </summary>
        //机构资信变动 = 1,
        /// <summary>
        /// 成品资料变动消息
        /// <remarks>不包含新增消息</remarks>
        /// </summary>
        成品资料变动 = 2,
        发货单 = 4,
        调拨单 = 8,
        零售单 = 16,
        退货单 = 64,
        订单变动 = 128
    }
}
