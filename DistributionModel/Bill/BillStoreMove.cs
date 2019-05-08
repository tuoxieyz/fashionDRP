using System;
using System.Text;
using System.Collections.Generic;
using System.Data;

namespace DistributionModel
{
    /// <summary>
    /// 移库单
    /// </summary>
    public class BillStoreMove : BillWithBrand
    {
        public int StorageIDOut { get; set; }
        public int StorageIDIn { get; set; }
    }
}

