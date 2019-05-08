using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class ABCEntity
    {
        public string StyleCode { get; set; }
        public int ColorID { get; set; }
        public string ColorName { get; set; }
        public int NameID { get; set; }
        public string Name { get; set; }
        public decimal CostMoney { get; set; }//零售单时，为减去了CutMoney后的金额
        public int Quantity { get; set; }
        public decimal Proportion { get; set; }
        public decimal AccuProportion { get; set; }//累计占比
    }
}
