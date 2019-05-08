using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionModel
{
    /// <summary>
    /// 订单变动
    /// </summary>
    public class BillOrderChange
    {
        public int BillID { get; set; }
        public string Description { get; set; }
        public int CreatorID { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
