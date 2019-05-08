using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;
using DistributionModel;
using DBAccess;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class RetailShoppingGuideBO : RetailShoppingGuide, INotifyPropertyChanged, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.DistributionQuery.LinqOP;

        private DateTime _onBoardDate = DateTime.Now;
        /// <summary>
        /// 入职时间
        /// </summary>
        public override DateTime OnBoardDate
        {
            get { return _onBoardDate; }
            set
            {
                if (_onBoardDate != value)
                {
                    _onBoardDate = value;
                    OnPropertyChanged("OnBoardDate");
                    OnPropertyChanged("DimissionDate");
                }
            }
        }

        private DateTime? _dimissionDate = null;
        /// <summary>
        /// 离职时间
        /// </summary>
        public override DateTime? DimissionDate
        {
            get { return _dimissionDate; }
            set
            {
                if (_dimissionDate != value)
                {
                    _dimissionDate = value;
                    OnPropertyChanged("DimissionDate");
                    OnPropertyChanged("OnBoardDate");
                }
            }
        }

        public RetailShoppingGuideBO()
        { }

        public RetailShoppingGuideBO(RetailShoppingGuide guide)
        {
            this.ID = guide.ID;
            this.Name = guide.Name;
            this.CreateTime = guide.CreateTime;
            Code = guide.Code;
            DimissionDate = guide.DimissionDate;
            OnBoardDate = guide.OnBoardDate;
            OrganizationID = guide.OrganizationID;
            ShiftID = guide.ShiftID;
            State = guide.State;
            CreatorID = guide.CreatorID;
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

            if (columnName == "Code")
            {
                if (string.IsNullOrEmpty(Code))
                    errorInfo = "不能为空";
                else if (Code.Length > 3)
                    errorInfo = "长度不能超过三位";
                else
                {
                    if (ID == 0)//新增
                    {
                        if (_linqOP.Any<RetailShoppingGuide>(e => e.OrganizationID == OrganizationID && e.Code == Code))
                            errorInfo = "该编号已经被使用";
                    }
                    else//编辑
                    {
                        if (_linqOP.Any<RetailShoppingGuide>(e => e.OrganizationID == OrganizationID && e.Code == Code && e.ID != ID))
                            errorInfo = "该编号已经被使用";
                    }
                }
            }
            else if (string.IsNullOrEmpty(errorInfo))
            {
                if (columnName == "DimissionDate" || columnName == "OnBoardDate")
                {
                    if (DimissionDate != null && DimissionDate.Value <= OnBoardDate)
                        errorInfo = "离职日期必须大于入职日期";
                }
            }
            return errorInfo;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
