using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace DistributionModel
{
    /// <summary>
    /// 盈亏单
    /// </summary>
    public class BillStocktakeContrast : BillWithBrand, IStorageID
    {
        public int StorageID { get; set; }
    }
}

