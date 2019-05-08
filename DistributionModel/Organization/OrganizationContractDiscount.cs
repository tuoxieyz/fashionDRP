using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace DistributionModel
{
    public class OrganizationContractDiscount : CreatedData, IDataErrorInfo, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public virtual int OrganizationID { get; set; }
        public int BYQID { get; set; }
        public decimal Discount { get; set; }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "OrganizationID")
            {
                if (OrganizationID == default(int))
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
