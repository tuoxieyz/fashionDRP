using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class SafetyTechForCertificationBO : CertSafetyTech, IDataErrorInfo
    {
        private DataChecker _checker;

        public SafetyTechForCertificationBO()
        { }

        public SafetyTechForCertificationBO(CertSafetyTech safetyTech)
        {
            this.ID = safetyTech.ID;
            this.Name = safetyTech.Name;
            this.Code = safetyTech.Code;
            CreateTime = safetyTech.CreateTime;
            CreatorID = safetyTech.CreatorID;
            this.Enabled = safetyTech.Enabled;
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
                errorInfo = _checker.CheckDataCodeName<CertSafetyTech>(this, columnName);
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
