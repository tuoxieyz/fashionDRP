using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using ERPViewModelBasic;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class VIPUpTacticValid : VIPUpTactic, IDataErrorInfo
    {
        private DataChecker _checker;

        public VIPUpTacticValid()
        { }

        public VIPUpTacticValid(VIPUpTactic tactic)
        {
            this.ID = tactic.ID;
            this.AfterKindID = tactic.AfterKindID;
            BrandID = tactic.BrandID;
            CutPoint = tactic.CutPoint;
            DateSpan = tactic.DateSpan;
            FormerKindID = tactic.FormerKindID;
            IsEnabled = tactic.IsEnabled;
            Name = tactic.Name;
            OnceConsume = tactic.OnceConsume;
            SpanConsume = tactic.SpanConsume;
            CreateTime = tactic.CreateTime;
            CreatorID = tactic.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;
            if (columnName == "Name")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.DistributionQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataName<VIPUpTactic>(this);
            }
            else if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "FormerKindID")
            {
                if (FormerKindID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "AfterKindID")
            {
                if (AfterKindID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "OnceConsume")
            {
                if (OnceConsume < 0)
                    errorInfo = "不能小于0";
            }
            else if (columnName == "DateSpan")
            {
                if (DateSpan < 0)
                    errorInfo = "不能小于0";
            }
            else if (columnName == "SpanConsume")
            {
                if (SpanConsume < 0)
                    errorInfo = "不能小于0";
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
