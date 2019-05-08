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
    public class RetailTacticBO : RetailTactic, IDataErrorInfo
    {
        public RetailTacticBO()
        { }

        public RetailTacticBO(RetailTactic tactic)
        {
            this.ID = tactic.ID;
            this.Name = tactic.Name;
            OrganizationID = tactic.OrganizationID;
            this.BeginDate = tactic.BeginDate;
            BrandID = tactic.BrandID;
            CanVIPApply = tactic.CanVIPApply;
            CostMoney = tactic.CostMoney;
            CutMoney = tactic.CutMoney;
            Discount = tactic.Discount;
            EndDate = tactic.EndDate;
            CreateTime = tactic.CreateTime;
            CreatorID = tactic.CreatorID;
            Kind = tactic.Kind;
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
                    if (VMGlobal.DistributionQuery.LinqOP.Any<RetailTactic>(e => e.OrganizationID == OrganizationID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (VMGlobal.DistributionQuery.LinqOP.Any<RetailTactic>(e => e.OrganizationID == OrganizationID && e.ID != ID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
            }
            else if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "Kind")
            {
                if (Kind == default(int))
                    errorInfo = "不能为空";
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
