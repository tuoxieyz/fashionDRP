using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace DistributionModel
{
    //BillRetailDetails
    public class BillRetailDetails : BillDetailBase
    {
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
        /// <summary>
        /// 减去金额(在应用了满减策略时和退货时有用)
        /// </summary>
        public decimal CutMoney { get; set; }
    }
}

