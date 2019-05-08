using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class MaterielForCertificationBO : CertMateriel, IDataErrorInfo
    {
        private DataChecker _checker;

        public MaterielForCertificationBO()
        { }

        public MaterielForCertificationBO(CertMateriel materiel)
        {
            this.ID = materiel.ID;
            this.Name = materiel.Name;
            this.Code = materiel.Code;
            CreateTime = materiel.CreateTime;
            CreatorID = materiel.CreatorID;
            this.Enabled = materiel.Enabled;
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
                errorInfo = _checker.CheckDataCodeName<CertMateriel>(this, columnName);
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
