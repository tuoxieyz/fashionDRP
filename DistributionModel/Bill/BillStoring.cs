using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Linq;
namespace DistributionModel
{
    /// <summary>
    /// 入库单
    /// </summary>
    public class BillStoring : BillWithBrand, IStorageID
    {
        public int StorageID { get; set; }
        /// <summary>
        /// 与入库单关联的单据编号
        /// </summary>
        public string RefrenceBillCode { get; set; }
        /// <summary>
        /// 与入库单关联的单据类型（比如该入库单是收货入库还是调拨入库，那就涉及到相关的配货单和调拨单）
        /// 为0就是入库单自己
        /// </summary>
        public int BillType { get; set; }
    }
}

