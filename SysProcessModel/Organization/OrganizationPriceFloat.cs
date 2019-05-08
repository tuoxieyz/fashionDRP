using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using System.ComponentModel;
using Model.Extension;

namespace SysProcessModel
{
    public class OrganizationPriceFloat : CreatedData, IDataErrorInfo, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int OrganizationID { get; set; }
        public int BYQID { get; set; }       
        public decimal FloatRate { get; set; }
        public int LastNumber { get; set; }

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
