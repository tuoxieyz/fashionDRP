using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using System.ComponentModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class FactoryBO : Factory, IDataErrorInfo, INotifyPropertyChanged
    {
        private DataChecker _checker;

        public string KindName
        {
            get
            {
                return OuterFactoryVM.Kinds.Find(o => o.Flag == IsOuter).Name;
            }
        }

        public override bool IsOuter
        {
            get
            {
                return base.IsOuter;
            }
            set
            {
                if (base.IsOuter != value)
                {
                    base.IsOuter = value;
                    OnPropertyChanged("KindName");
                }
            }
        }

        public FactoryBO()
        { }

        public FactoryBO(Factory factory)
        {
            this.ID = factory.ID;
            this.Name = factory.Name;
            this.Code = factory.Code;
            this.LinkMan = factory.LinkMan;
            this.LinkPhone = factory.LinkPhone;
            this.Remark = factory.Remark;
            this.IsEnabled = factory.IsEnabled;
            CreateTime = factory.CreateTime;
            CreatorID = factory.CreatorID;
            IsOuter = factory.IsOuter;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name" || columnName == "Code")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.ManufacturingQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataCodeName<Factory>(this, columnName);
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
