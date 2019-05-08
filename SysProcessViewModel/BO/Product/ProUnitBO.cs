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
    public class ProUnitBO : ProUnit, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProUnitBO()
        { }

        public ProUnitBO(ProUnit unit)
        {
            this.ID = unit.ID;
            this.Name = unit.Name;
            this.Flag = unit.Flag;
            CreateTime = unit.CreateTime;
            CreatorID = unit.CreatorID;
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
                errorInfo = _checker.CheckDataName<ProUnit>(this);
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
