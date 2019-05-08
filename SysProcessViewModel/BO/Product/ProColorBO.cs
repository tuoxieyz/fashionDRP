using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using Model.Extension;
using System.ComponentModel;

using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProColorBO : ProColor, IDataErrorInfo
    {
        private DataChecker _checker;

        public ProColorBO()
        { }

        public ProColorBO(ProColor color)
        {
            this.ID = color.ID;
            this.Name = color.Name;
            this.RGBCode = color.RGBCode;
            Code = color.Code;
            CreateTime = color.CreateTime;
            CreatorID = color.CreatorID;
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
                errorInfo = _checker.CheckDataCodeName<ProColor>(this, columnName);
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

    public class ProColorForSet : ProColor, INotifyPropertyChanged
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
