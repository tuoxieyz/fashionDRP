using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class OrganizationPriceFloatBO : OrganizationPriceFloat
    {
        public int BrandID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }

        protected override string CheckData(string columnName)
        {
            var errorInfo = base.CheckData(columnName);

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
    }
}
