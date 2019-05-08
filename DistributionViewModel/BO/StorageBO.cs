using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;
using DBAccess;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class StorageBO : Storage, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.DistributionQuery.LinqOP;

        public StorageBO()
        { }

        public StorageBO(Storage storage)
        {
            this.ID = storage.ID;
            this.Name = storage.Name;
            this.OrganizationID = storage.OrganizationID;
            Flag = storage.Flag;
            CreateTime = storage.CreateTime;
            CreatorID = storage.CreatorID;
        }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name")
            {
                if (string.IsNullOrWhiteSpace(Name))
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<Storage>(o => o.OrganizationID == OrganizationID && o.Name == Name))
                        errorInfo = "已存在相同名称的仓库";
                }
                else//编辑
                {
                    if (_linqOP.Any<Storage>(o => o.OrganizationID == OrganizationID && o.Name == Name && o.ID != ID))
                        errorInfo = "该名称已经被使用";
                }
            }

            return errorInfo;
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
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
