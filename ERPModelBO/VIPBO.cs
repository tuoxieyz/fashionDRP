using DistributionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    public class VIPBO
    {
        public VIPCard CardInfo { get; set; }

        public int CardPoint { get; set; }

        public List<VIPKind> Kinds { get; set; }

        public VIPBirthdayConsumption BirthdayConsumption { get; set; }
    }
}
