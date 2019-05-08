using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    [Flags]
    public enum RetailTacticKind
    {
        满减策略 = 1,
        折扣策略 = 2,
        混合策略 = 3
    }
}
