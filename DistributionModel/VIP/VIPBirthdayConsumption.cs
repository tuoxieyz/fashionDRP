using DBLinqProvider.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionModel
{
    public class VIPBirthdayConsumption
    {
        public int VIPID { get; set; }
        public int Quantity { get; set; }
        public int CostMoney { get; set; }
        /// <summary>
        /// 表明在哪一年的生日消费
        /// </summary>
        public DateTime ConsumeDay { get; set; }
    }
}
