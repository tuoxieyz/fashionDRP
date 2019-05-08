using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace DistributionModel
{
    /// <summary>
    /// 配货单
    /// </summary>
    public class BillAllocate : BillBase
    {
        public int StorageID { get; set; }
        /// <summary>
        /// false：未完成 true：已完成
        /// </summary>
        public bool Status { get; set; }

        public int Quantity { get; set; }

        public int? HandlerID { get; set; }

        public DateTime? HandleTime { get; set; }
    }
}

