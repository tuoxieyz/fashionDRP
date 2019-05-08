using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace DistributionModel
{
    /// <summary>
    /// 出库单
    /// </summary>
    public class BillStoreOut : BillWithBrand, IStorageID
    {
        public int StorageID { get; set; }
        public string RefrenceBillCode { get; set; }
        public int BillType { get; set; }
    }
}

