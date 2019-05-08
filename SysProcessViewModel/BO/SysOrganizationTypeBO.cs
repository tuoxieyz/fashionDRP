using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;
using SysProcessModel;
using DBAccess;

namespace SysProcessViewModel
{
    public class SysOrganizationTypeBO : SysOrganizationType, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.SysProcessQuery.LinqOP;

        public SysOrganizationTypeBO()
        { }

        public SysOrganizationTypeBO(SysOrganizationType otype)
        {
            this.ID = otype.ID;
            this.Name = otype.Name;
            this.OrganizationID = otype.OrganizationID;
            this.IsEnabled = otype.IsEnabled;
            this.CreateTime = otype.CreateTime;
            this.CreatorID = otype.CreatorID;
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

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name")
            {
                if (string.IsNullOrWhiteSpace(Name))
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<SysOrganizationType>(e => e.OrganizationID == OrganizationID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<SysOrganizationType>(e => e.OrganizationID == OrganizationID && e.ID != ID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
            }
            return errorInfo;
        }
    }
}
