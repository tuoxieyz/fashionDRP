using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using ViewModelBasic;

using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProQuarterBO : ProQuarter, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProQuarterBO()
        { }

        public ProQuarterBO(ProQuarter quarter)
        {
            this.ID = quarter.ID;
            this.Name = quarter.Name;
            this.CreateTime = quarter.CreateTime;
            IsEnabled = quarter.IsEnabled;
            CreatorID = quarter.CreatorID;
        }        

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.SysProcessQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataName<ProQuarter>(this);
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
