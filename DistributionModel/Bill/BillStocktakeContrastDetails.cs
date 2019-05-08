using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace DistributionModel
{
    //BillStocktakeContrastDetails
    public class BillStocktakeContrastDetails : BillDetailBase
    {
        /// <summary>
        /// 盘点数量
        /// </summary>
        public int QuaStocktake { get; set; }
        /// <summary>
        /// 原库存数
        /// </summary>
        public int QuaStockOrig { get; set; }
    }
}

