using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace DistributionModel
{
    //BillGoodReturnDetails
    public class BillGoodReturnDetails : BillDetailBase
    {
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
    }
}

