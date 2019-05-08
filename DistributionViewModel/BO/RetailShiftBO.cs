using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using Kernel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class RetailShiftBO : RetailShift, IDataErrorInfo
    {
        public RetailShiftBO()
        { }

        public RetailShiftBO(RetailShift shift)
        {
            this.ID = shift.ID;
            this.Name = shift.Name;
            OrganizationID = shift.OrganizationID;
            this.IsEnabled = shift.IsEnabled;
            CreateTime = shift.CreateTime;
            CreatorID = shift.CreatorID;
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
                if (Name.IsNullEmpty())
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (VMGlobal.DistributionQuery.LinqOP.Any<RetailShift>(e => e.OrganizationID == OrganizationID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (VMGlobal.DistributionQuery.LinqOP.Any<RetailShift>(e => e.OrganizationID == OrganizationID && e.ID != ID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
            }

            return errorInfo;
        }
    }
}
