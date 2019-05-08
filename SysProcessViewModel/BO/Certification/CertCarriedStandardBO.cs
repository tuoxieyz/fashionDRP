using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class CarriedStandardForCertificationBO : CertCarriedStandard, IDataErrorInfo
    {
        private DataChecker _checker;

        public CarriedStandardForCertificationBO()
        { }

        public CarriedStandardForCertificationBO(CertCarriedStandard carriedStandard)
        {
            this.ID = carriedStandard.ID;
            this.Name = carriedStandard.Name;
            this.Code = carriedStandard.Code;
            CreateTime = carriedStandard.CreateTime;
            CreatorID = carriedStandard.CreatorID;
            this.Enabled = carriedStandard.Enabled;
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
                errorInfo = _checker.CheckDataCodeName<CertCarriedStandard>(this, columnName);
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
