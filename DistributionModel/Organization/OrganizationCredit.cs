using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using System.ComponentModel;
using Model.Extension;

namespace DistributionModel
{
    /// <summary>
    /// 客户(加盟商、代理商等)资信，自营店无资信一说
    /// </summary>
    public class OrganizationCredit : CreatedData, IDataErrorInfo,IDEntity,INotifyPropertyChanged
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }

        public int OrganizationID { get; set; }
        public int BrandID { get; set; }
        public int CreditMoney { get; set; }

        private DateTime _endDate = DateTime.MaxValue;
        /// <summary>
        /// 资信有效期
        /// </summary>
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; } }

        public string Remark { get; set; }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "OrganizationID")
            {
                if (OrganizationID == default(int))
                    errorInfo = "不能为空";                
            }
            else if (columnName == "BrandID")
            {
                if (BrandID == default(int))
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
