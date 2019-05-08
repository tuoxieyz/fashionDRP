using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Linq;
namespace DistributionModel
{
    /// <summary>
    /// 发货单
    /// </summary>
    public class BillDelivery : BillWithBrand, IStorageID
    {
        public int StorageID { get; set; }
        public int ToOrganizationID { get; set; }
        public string BillAllocateCode { get; set; }
        /// <summary>
        /// 发货状态:0在途中;1已收货入库;2已装箱未配送
        /// </summary>
        public virtual int Status { get; set; }
        
        private bool _isWriteDownOrder = true;
        /// <summary>
        /// 是否冲减订单
        /// </summary>
        public bool IsWriteDownOrder { get { return _isWriteDownOrder; } set { _isWriteDownOrder = value; } }

        /// <summary>
        /// 发货类型 0:正常发货 1:折价发货
        /// </summary>
        public int DeliveryKind { get; set; }
    }
}

