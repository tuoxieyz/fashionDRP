using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SysProcessViewModel
{
    /// <summary>
    /// 基础资料和财务信息(在设置操作权限时使用)
    /// </summary>
    [Flags]
    public enum BasicInfoEnum
    {
        //只读 = 0,
        分支机构 = 1,
        成品资料 = 2,
        财务扣款 = 4,
        财务收款 = 8,
        机构资信 = 16,
        VIP类型 = 32,
        VIP策略 = 64,
        VIP信息 = 128,
        零售策略 = 256
    }
}
