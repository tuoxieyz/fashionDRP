using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Model.Extension
{
    public class BillDetailBase
    {
        public int BillID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}
