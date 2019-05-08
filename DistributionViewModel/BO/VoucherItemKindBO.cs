using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Kernel;
using System.ComponentModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class VoucherItemKindBO : VoucherItemKind, IDataErrorInfo
    {
        public VoucherItemKindBO()
        { }

        public VoucherItemKindBO(VoucherItemKind kind)
        {
            this.ID = kind.ID;
            this.Name = kind.Name;
            OrganizationID = kind.OrganizationID;
            this.IsEnabled = kind.IsEnabled;
            CreateTime = kind.CreateTime;
            CreatorID = kind.CreatorID;
            Kind = kind.Kind;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name")
            {
                if (Name.IsNullEmpty())
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (VMGlobal.DistributionQuery.LinqOP.Any<VoucherItemKind>(e => e.OrganizationID == OrganizationID && e.Name == Name && e.Kind == Kind))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (VMGlobal.DistributionQuery.LinqOP.Any<VoucherItemKind>(e => e.OrganizationID == OrganizationID && e.ID != ID && e.Name == Name && e.Kind == Kind))
                        errorInfo = "该名称已经被使用";
                }
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
