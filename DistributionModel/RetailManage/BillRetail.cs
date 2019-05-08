using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using Model.Extension;
namespace DistributionModel
{
    /// <summary>
    /// 零售单
    /// </summary>
    public class BillRetail : BillBase, IDataErrorInfo
    {
        public int StorageID { get; set; }
        public int? VIPID { get; set; }
        public int Quantity { get; set; }
        public decimal CostMoney { get; set; }
        /// <summary>
        /// 预存款支付
        /// </summary>
        public decimal PredepositPay { get; set; }
        public decimal ReceiveMoney { get; set; }
        public int ReceiveTicket { get; set; }
        public decimal TicketMoney { get; set; }
        /// <summary>
        /// 消费券类型:1折前券2折后券3两者都有
        /// </summary>
        public int? TicketKind { get; set; }
        /// <summary>
        /// 导购ID
        /// </summary>
        public int? GuideID { get; set; }
        /// <summary>
        /// 班次
        /// </summary>
        public int? ShiftID { get; set; }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "StorageID")
            {
                if (StorageID == default(int))
                    errorInfo = "不能为空";
            }

            return errorInfo;
        }

        string IDataErrorInfo.Error
        {
            get { return ""; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return this.CheckData(columnName);
            }
        }
    }
}

