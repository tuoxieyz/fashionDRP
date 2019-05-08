using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class GradeForCertificationBO : CertGrade, IDataErrorInfo
    {
        private DataChecker _checker;

        public GradeForCertificationBO()
        { }

        public GradeForCertificationBO(CertGrade grade)
        {
            this.ID = grade.ID;
            this.Name = grade.Name;
            this.Enabled = grade.Enabled;
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
                errorInfo = _checker.CheckDataName<CertGrade>(this);
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
