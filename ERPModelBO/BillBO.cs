using Model.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    public class BillBO<TBill, TDetail>
        where TBill : BillBase
        where TDetail : BillDetailBase
    {
        public TBill Bill { get; set; }
        public List<TDetail> Details { get; set; }
    }
}
