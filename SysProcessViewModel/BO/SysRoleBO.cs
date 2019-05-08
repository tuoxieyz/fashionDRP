using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using DBAccess;
using ViewModelBasic;
using SysProcessModel;
using DomainLogicEncap;

namespace SysProcessViewModel
{
    public class SysRoleBO : SysRole, IDataErrorInfo
    {
        private DataChecker _checker;

        private List<SysModule> _modules;
        public List<SysModule> Modules
        {
            get
            {
                if (_modules == null)
                {
                    _modules = RoleLogic.ModuleProcessOfRole(this.ID);
                }
                return _modules;
            }
            set
            {
                _modules = value;
            }
        }

        public SysRoleBO()
        { }

        public SysRoleBO(SysRole role)
        {
            this.ID = role.ID;
            this.Name = role.Name;
            this.Description = role.Description;
            this.OrganizationID = role.OrganizationID;
            this.OPAccess = role.OPAccess;
            this.IMAccess = role.IMAccess;
            CreateTime = role.CreateTime;
            CreatorID = role.CreatorID;
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
                errorInfo = _checker.CheckDataName<SysRole>(this);
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
