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
    public class ProBoduanBO : ProBoduan, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProBoduanBO()
        { }

        public ProBoduanBO(ProBoduan boduan)
        {
            this.ID = boduan.ID;
            this.Name = boduan.Name;
            this.CreateTime = boduan.CreateTime;
            IsEnabled = boduan.IsEnabled;
            CreatorID = boduan.CreatorID;
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
                errorInfo = _checker.CheckDataName<ProBoduan>(this);
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
