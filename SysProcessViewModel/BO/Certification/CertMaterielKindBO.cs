using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class MaterielKindForCertificationBO: CertMaterielKind, IDataErrorInfo
    {
        private DataChecker _checker;

        public MaterielKindForCertificationBO()
        { }

        public MaterielKindForCertificationBO(CertMaterielKind materiel)
        {
            this.ID = materiel.ID;
            this.Name = materiel.Name;
            this.Code = materiel.Code;
            CreateTime = materiel.CreateTime;
            CreatorID = materiel.CreatorID;
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
                errorInfo = _checker.CheckDataCodeName<CertMaterielKind>(this, columnName);
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
