using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SysProcessModel;
using DomainLogicEncap;

namespace SysProcessViewModel
{
    public class CertificationBO : Certification, IDataErrorInfo, INotifyPropertyChanged
    {
        static FloatPriceHelper _fpHelper = new FloatPriceHelper();

        public string StyleCode
        {
            get
            {
                return this.Style == null ? "" : this.Style.Code;
            }
            set
            {
                if (value != StyleCode)
                {
                    var style = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle>(o => o.Code == value).FirstOrDefault();
                    if (style == null)
                        Style = new ProStyleBO();
                    else
                        Style = new ProStyleBO(style);
                    StyleID = Style.ID;
                    Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, Style.BYQID, Style.Price);
                    OnPropertyChanged("Style");
                }
            }
        }

        public ProStyleBO Style { get; set; }

        public override decimal Price
        {
            get
            {
                return base.Price;
            }
            set
            {
                base.Price = value;
                OnPropertyChanged("Price");
            }
        }

        public string GradeName { get; set; }
        public string CarriedStandardName { get; set; }
        public string SafetyTechniqueName { get; set; }

        private string _barCode = "1234567890";
        public string BarCode
        {
            get { return _barCode; }
            set
            {
                _barCode = value;
                OnPropertyChanged("BarCode");
            }
        }

        private string _color = "000[无色]";
        //[DefaultValue("000[无色]")]//DefaultValue貌似是给设计器用的，汗！
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }
        private string _size = "00[全码]";
        public string Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

        /// <summary>
        /// 打印数量
        /// </summary>
        public int Quantity { get; set; }

        public CertificationBO() { }

        public CertificationBO(Certification certification)
        {
            this.ID = certification.ID;
            this.Price = certification.Price;
            this.StyleID = certification.StyleID;
            this.SafetyTechnique = certification.SafetyTechnique;
            this.Composition = certification.Composition;
            this.Grade = certification.Grade;
            this.CarriedStandard = certification.CarriedStandard;
            this.GBCode = certification.GBCode;
            this.CreateTime = certification.CreateTime;
            this.CreatorID = certification.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            switch (columnName)
            {
                case "Grade":
                    if (Grade == default(int))
                        errorInfo = "不能为空";
                    break;
                case "SafetyTechnique":
                    if (SafetyTechnique == default(int))
                        errorInfo = "不能为空";
                    break;
                case "CarriedStandard":
                    if (CarriedStandard == default(int))
                        errorInfo = "不能为空";
                    break;
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
