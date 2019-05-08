using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using ViewModelBasic;

using System.ComponentModel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProSizeBO : ProSize, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProSizeBO()
        { }

        public ProSizeBO(ProSize size)
        {
            this.ID = size.ID;
            this.Name = size.Name;
            this.CreateTime = size.CreateTime;
            Code = size.Code;
            Flag = size.Flag;
            CreatorID = size.CreatorID;
            Description = size.Description;
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
                errorInfo = _checker.CheckDataCodeName<ProSize>(this, columnName);
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

    /// <summary>
    /// 便于界面列表绑定时区分Size是否被占用的扩展类
    /// </summary>
    public class ProSizeForSet : ProSize, INotifyPropertyChanged
    {
        private bool _isHold = false;

        public bool IsHold
        {
            get { return _isHold; }
            set
            {
                if (_isHold != value)
                {
                    _isHold = value;
                    OnPropertyChanged("IsHold");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
