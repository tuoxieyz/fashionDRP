using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Kernel;

namespace SysProcessViewModel
{
    public class BatchPriceFloat : IDataErrorInfo
    {
        public int BrandID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal FloatRate { get; set; }
        public int LastNumber { get; set; }
        public List<int> OrganizationIDs { get; set; }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "Year")
            {
                if (Year == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "Quarter")
            {
                if (Quarter == default(int))
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
