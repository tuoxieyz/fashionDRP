using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;

namespace DistributionModel.Finance
{
    /// <summary>
    /// 收款单
    /// </summary>
    public class VoucherReceiveMoney : BillBase, IDataErrorInfo
    {
        public int BrandID { get; set; }
        public decimal ReceiveMoney { get; set; }

        private DateTime _occurDate = DateTime.Now;
        public DateTime OccurDate
        {
            get { return _occurDate; }
            set { _occurDate = value; }
        }
        /// <summary>
        /// 收款项目编号
        /// </summary>
        public int ReceiveKindID { get; set; }

        private bool _status = false;

        /// <summary>
        /// 是否已审核
        /// </summary>
        public bool Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                }
            }
        }

        /// <summary>
        /// 款项是否冻结(即是否能作为货款使用)
        /// </summary>
        public bool IsMoneyFrozen { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public int CheckerID { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? CheckTime { get; set; }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "ReceiveMoney")
            {
                if (ReceiveMoney <= 0)
                    errorInfo = "必须大于0";
            }
            else if (columnName == "ReceiveKindCode")
            {
                if (ReceiveKindID==default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "OrganizationID")
            {
                if (OrganizationID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "ReceiveKindID")
            {
                if (ReceiveKindID == default(int))
                    errorInfo = "不能为空";
            }

            return errorInfo;
        }


        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return this.CheckData(columnName);
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
    }
}
