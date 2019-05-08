using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatusEnum
    {
        /// <summary>
        /// 未发货or未配货(假如强制走配货流程的话)
        /// </summary>
        NotDelivered = 0,
        /// <summary>
        /// 部分已发货
        /// </summary>
        PortionDelivered = 1,
        /// <summary>
        /// 发货完毕
        /// </summary>
        AllDelivered = 2
    }
}
