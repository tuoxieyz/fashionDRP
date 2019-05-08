using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    /// <summary>
    /// 单据编号前缀
    /// 和单据类型BillTypeEnum一一对应
    /// </summary>
    public enum BillCodePrefixion
    {
        RK = 0,
        DH = 1,
        PH = 2,
        FH = 3,
        CK = 4,
        PD = 5,
        YK = 6,
        ZK = 7,//移库和盈亏两者拼音首字母一样，那移库取Y后一个字母Z作为第一个字母
        LS = 8,
        DB = 9,
        KK = 10,
        SK = 11,
        TH = 12,
        JJ = 13,
        WT = 14,
        JH = 15
    }
}
