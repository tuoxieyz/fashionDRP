using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using ViewModelBasic;
using System.ComponentModel;

using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProNameBO : ProName, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProNameBO()
        { }

        public ProNameBO(ProName name)
        {
            this.ID = name.ID;
            this.Name = name.Name;
            this.Flag = name.Flag;
            CreateTime = name.CreateTime;
            CreatorID = name.CreatorID;
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
                errorInfo = _checker.CheckDataName<ProName>(this);
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
