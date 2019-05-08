using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FactoryProductionModel;
using System.ComponentModel;
using DistributionModel;
using DBAccess;
using ViewModel.Extension;

namespace Manufacturing.ViewModel
{
    public class CertificationBO : Certification, IDataErrorInfo, INotifyPropertyChanged
    {
        private LinqOPEncap _distributionLinqOP = VMGlobalBase.DistributionQuery.LinqOP;

        public override int ProductID
        {
            get
            {
                return base.ProductID;
            }
            set
            {
                base.ProductID = value;
                if (value == default(int))
                {
                    StyleCode = "";//不知赋值会否引起执行CheckData方法
                }
                else
                {
                    var product = _distributionLinqOP.GetById<Product>(value);
                    if (product == null)
                    {
                        StyleCode = "";
                    }
                    else
                    {
                        StyleCode = _distributionLinqOP.GetById<ProStyle>(product.StyleID).Code;
                    }
                }
            }
        }

        private string _styleCode;
        public string StyleCode
        {
            get { return _styleCode; }
            set
            {
                if (_styleCode != value)
                {
                    _styleCode = value;
                }
            }
        }

        private int _styleID;
        public int StyleID
        {
            get { return _styleID; }
            set
            {
                _styleID = value;
                if (_styleID == default(int))
                {
                    Colors = null;
                    Sizes = null;
                }
                else
                {
                    var products = _distributionLinqOP.Search<Product>(p => p.StyleID == _styleID).ToList();
                    Colors = VMGlobalBase.Colors.Where(o => products.Select(p => p.ColorID).Contains(o.ID));
                    Sizes = VMGlobalBase.Sizes.Where(o => products.Select(p => p.SizeID).Contains(o.ID));
                }
                OnPropertyChanged("Colors");
                OnPropertyChanged("Sizes");
            }
        }

        private string _styleName;
        public string StyleName
        {
            get { return _styleName; }
            set
            {
                _styleName = value;
                OnPropertyChanged("StyleName");
            }
        }

        private string _brandName;
        public string BrandName
        {
            get { return _brandName; }
            set
            {
                _brandName = value;
                OnPropertyChanged("BrandName");
            }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
            }
        }

        public IEnumerable<ProColor> Colors
        {
            get;
            set;
        }

        public IEnumerable<ProSize> Sizes
        {
            get;
            set;
        }

        private int _colorID;
        public int ColorID
        {
            get { return _colorID; }
            set
            {
                _colorID = value;
                OnPropertyChanged("ColorName");
            }
        }

        private int _sizeID;
        public int SizeID
        {
            get { return _sizeID; }
            set
            {
                _sizeID = value;
                OnPropertyChanged("SizeName");
            }
        }

        public string ColorName
        {
            get
            {
                var color = VMGlobalBase.Colors.Find(o => o.ID == this.ColorID);
                return color == null ? "" : color.Name;
            }
        }

        public string SizeName
        {
            get
            {
                var size = VMGlobalBase.Sizes.Find(o => o.ID == this.SizeID);
                return size == null ? "" : size.Name;
            }
        }

        public CertificationBO() { }

        public CertificationBO(Certification certification)
        {
            this.ID = certification.ID;
            this.OrganizationID = certification.OrganizationID;
            this.ProductID = certification.ProductID;
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
                case "StyleCode":
                    {
                        if (string.IsNullOrEmpty(StyleCode))
                            errorInfo = "不能为空";
                        else
                        {
                            var style = _distributionLinqOP.Search<ProStyle>(o => o.Code == StyleCode).FirstOrDefault();
                            if (style == null)
                            {
                                errorInfo = "款号不正确";
                                StyleID = default(int);
                                StyleName = "";
                                BrandName = "";
                                Price = 0;
                            }
                            else
                            {
                                StyleID = style.ID;
                                StyleName = VMGlobalBase.ProNames.Find(o => o.ID == style.NameID).Name;
                                var brandID = _distributionLinqOP.GetById<ProBYQ>(style.BYQID).BrandID;
                                var brand = VMGlobalBase.PoweredBrands.Find(o => o.ID == brandID);
                                BrandName = brand == null ? "" : brand.Name;
                            }
                        }
                    }
                    break;
                case "ColorID":
                    if (ColorID == default(int))
                        errorInfo = "不能为空";
                    break;
                case "SizeID":
                    if (SizeID == default(int))
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
