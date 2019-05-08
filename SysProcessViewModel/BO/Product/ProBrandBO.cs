using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProBrandBO: ProBrand, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProBrandBO()
        { }

        public ProBrandBO(ProBrand brand)
        {
            this.ID = brand.ID;
            this.Name = brand.Name;
            this.Description = brand.Description;
            this.Flag = brand.Flag;
            Code = brand.Code;
            CreateTime = brand.CreateTime;
            CreatorID = brand.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name" || columnName == "Code")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.SysProcessQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataCodeName<ProBrand>(this, columnName);
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
