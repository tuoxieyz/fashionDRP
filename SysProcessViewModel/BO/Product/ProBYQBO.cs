using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DBAccess;

using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProBYQBO : IDataErrorInfo
    {
        public int ID { get; set; }
        public int BrandID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }

        public ProBYQBO()
        { }

        public ProBYQBO(ProBYQ byq)
        {
            this.ID = byq.ID;
            this.BrandID = byq.BrandID;
            Year = byq.Year;
            Quarter = byq.Quarter;
        }

        #region 类型转换操作符重载

        public static implicit operator ProBYQBO(ProBYQ byq)
        {
            return new ProBYQBO(byq);
        }

        public static implicit operator ProBYQ(ProBYQBO byq)
        {
            return new ProBYQ
            {
                ID = byq.ID,
                BrandID = byq.BrandID,
                Year = byq.Year,
                Quarter = byq.Quarter
            };
        }

        #endregion

        private string CheckData(string columnName)
        {
            string errorInfo = null;
            if (columnName == "Quarter")
            {
                if (Quarter == 0)
                    errorInfo = "季度必选";
            }
            else if (columnName == "BrandID")
            {
                if (BrandID == 0)
                    errorInfo = "品牌必选";
            }
            else if (columnName == "Year")
            {
                if (Year == default(int))
                    errorInfo = "年份必选";
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
    }
}
