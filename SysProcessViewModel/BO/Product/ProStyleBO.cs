using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using DBAccess;
using SysProcessModel;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace SysProcessViewModel
{
    public class ProStyleBO : ProStyle, IDataErrorInfo, INotifyPropertyChanged//, IEditableObject
    {
        private LinqOPEncap _linqOP = VMGlobal.SysProcessQuery.LinqOP;
        //private ProStyleData _styleData;
        //private ProStyleData _backupData;

        //private class ProStyleData : ProStyle
        //{
        //    public int BrandID { get; set; }
        //    public string Year { get; set; }
        //    public int Quarter { get; set; }
        //    //public string BrandName { get; set; }
        //    //public string BrandCode { get; set; }
        //    //public string BoduanName { get; set; }
        //    //public string UnitName { get; set; }
        //    //public string Name { get; set; }
        //    public IEnumerable<ProColor> Colors { get; set; }
        //    public IEnumerable<ProSize> Sizes { get; set; }
        //}

        #region 属性

        public override string Code
        {
            get { return base.Code; }
            set
            {
                if (base.Code != value)
                {
                    base.Code = value;
                    OnPropertyChanged("Code");
                }
            }
        }
        public override int NameID
        {
            get { return base.NameID; }
            set
            {
                base.NameID = value;
                OnPropertyChanged("Name");
            }
        }
        public override int UnitID
        {
            get { return base.UnitID; }
            set
            {
                base.UnitID = value;
                OnPropertyChanged("UnitName");
            }
        }
        public override int BoduanID
        {
            get { return base.BoduanID; }
            set
            {
                base.BoduanID = value;
                OnPropertyChanged("BoduanName");
            }
        }
        public override decimal Price
        {
            get { return base.Price; }
            set
            {
                base.Price = value;
                OnPropertyChanged("Price");
            }
        }
        public override decimal CostPrice
        {
            get { return base.CostPrice; }
            set
            {
                base.CostPrice = value;
                OnPropertyChanged("CostPrice");
            }
        }
        //public override string PictureUrl
        //{
        //    get { return base.PictureUrl; }
        //    set
        //    {
        //        if (base.PictureUrl != value)
        //        {
        //            base.PictureUrl = value;
        //            OnPropertyChanged("PictureUrl");
        //        }
        //    }
        //}

        //以下为扩展属性
        private int _brandID;
        public int BrandID
        {
            get { return _brandID; }
            set
            {
                _brandID = value;
                OnPropertyChanged("BrandName");
                OnPropertyChanged("BrandCode");
            }
        }
        private int _year = DateTime.Now.Year;
        public int Year
        {
            get { return _year; }
            set
            {
                _year = value;
                OnPropertyChanged("Year");
            }
        }
        private int _quarter;
        public int Quarter
        {
            get { return _quarter; }
            set
            {
                _quarter = value;
                OnPropertyChanged("QuarterName");
            }
        }
        public string QuarterName
        {
            get
            {
                var quarter = VMGlobal.Quarters.Find(o => o.ID == this.Quarter);
                return quarter == null ? "" : quarter.Name;
            }
        }
        public string BrandName
        {
            get
            {
                var brand = VMGlobal.PoweredBrands.Find(o => o.ID == this.BrandID);
                return brand == null ? "" : brand.Name;
            }
        }
        public string BrandCode
        {
            get
            {
                var brand = VMGlobal.PoweredBrands.Find(o => o.ID == this.BrandID);
                return brand == null ? "" : brand.Code;
            }
        }
        public string BoduanName
        {
            get
            {
                var boduan = VMGlobal.Boduans.Find(o => o.ID == this.BoduanID);
                return boduan == null ? "" : boduan.Name;
            }
        }
        public string UnitName
        {
            get
            {
                var unit = VMGlobal.Units.Find(o => o.ID == this.UnitID);
                return unit == null ? "" : unit.Name;
            }
        }
        public string Name
        {
            get
            {
                var proname = VMGlobal.ProNames.Find(o => o.ID == this.NameID);
                return proname == null ? "" : proname.Name;
            }
        }

        //public override string Description
        //{
        //    get
        //    {
        //        if (base.Description == null)
        //        {
        //            if (this.ID != default(int))
        //            {
        //                string description = _linqOP.Search<ProStyle, string>(o => o.Description, o => o.ID == this.ID).FirstOrDefault();
        //                if (description == null)
        //                    description = "";
        //                base.Description = description;
        //            }
        //        }
        //        return base.Description;
        //    }
        //    set
        //    {
        //        base.Description = value;
        //    }
        //}

        private IEnumerable<ProColor> _colors;
        public IEnumerable<ProColor> Colors
        {
            get
            {
                if (_colors == null)
                {
                    var products = _linqOP.GetDataContext<Product>();
                    var colors = _linqOP.GetDataContext<ProColor>();
                    var data = from p in products
                               from c in colors
                               where p.ColorID == c.ID && p.StyleID == this.ID
                               select c;
                    _colors = data.Distinct().ToList();
                }
                return _colors;
            }
            set
            {
                _colors = value;
                OnPropertyChanged("Colors");
            }
        }

        private IEnumerable<ProSize> _sizes;
        public IEnumerable<ProSize> Sizes
        {
            get
            {
                if (_sizes == null)
                {
                    var products = _linqOP.GetDataContext<Product>();
                    var sizes = _linqOP.GetDataContext<ProSize>();
                    var data = from p in products
                               from s in sizes
                               where p.SizeID == s.ID && p.StyleID == this.ID
                               select s;
                    _sizes = data.Distinct().ToList();
                }
                return _sizes;
            }
            set
            {
                _sizes = value;
                OnPropertyChanged("Sizes");
            }
        }

        private IEnumerable<ProSCPictureBO> _pictures;
        public IEnumerable<ProSCPictureBO> Pictures
        {
            get
            {
                if (_pictures == null && this.ID != default(int))
                {
                    //var scs = this.Colors.Select(o => this.Code + o.Code);
                    _pictures = VMGlobal.SysProcessQuery.LinqOP.Search<ProSCPicture>(o => o.StyleID == this.ID)
                        .Select(o => new ProSCPictureBO(o)).ToList();
                }
                return _pictures;
            }
        }

        private ObservableCollection<ProStyleChange> _changes;
        public ObservableCollection<ProStyleChange> Changes
        {
            get
            {
                if (_changes == null)
                {
                    _changes = new ObservableCollection<ProStyleChange>(_linqOP.Search<ProStyleChange>(o => o.StyleID == this.ID).OrderByDescending(o => o.CreateTime));
                }
                return _changes;
            }
        }

        #endregion

        public ProStyleBO()
        {
            //this._styleData = new ProStyleData();
        }

        public ProStyleBO(ProStyle style)
        {
            //this._styleData = new ProStyleData();
            this.ID = style.ID;
            this.Code = style.Code;
            this.CreatorID = style.CreatorID;
            this.CreateTime = style.CreateTime;
            this.BoduanID = style.BoduanID;
            this.BYQID = style.BYQID;
            this.Flag = style.Flag;
            this.NameID = style.NameID;
            //this.PictureUrl = style.PictureUrl;
            this.Price = style.Price;
            this.CostPrice = style.CostPrice;
            this.UnitID = style.UnitID;
            this.EANCode = style.EANCode;
            //this.Description = style.Description;
            var byq = VMGlobal.BYQs.Find(o => o.ID == style.BYQID);
            if (byq != null)
            {
                this.BrandID = byq.BrandID;
                this.Quarter = byq.Quarter;
                this.Year = byq.Year;
            }
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Code")
            {
                if (string.IsNullOrWhiteSpace(Code))
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<ProStyle>(entity => entity.Code == Code))
                        errorInfo = "该款号已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<ProStyle>(entity => entity.ID != ID && entity.Code == Code))
                        errorInfo = "该款号已经被使用";
                }
            }
            else if (columnName == "Quarter")
            {
                if (Quarter == 0)
                    errorInfo = "季度必选";
            }
            else if (columnName == "BrandID")
            {
                if (BrandID == 0)
                    errorInfo = "品牌必选";
            }
            else if (columnName == "Year")
            {
                if (Year == default(int))
                    errorInfo = "年份必选";
            }
            else if (columnName == "NameID")
            {
                if (NameID == 0)
                    errorInfo = "品名必选";
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //public void BeginEdit()
        //{
        //    this._backupData = _styleData;
        //}

        //public void CancelEdit()
        //{
        //    this._styleData = _backupData;
        //}

        //public void EndEdit()
        //{
        //    _backupData = new ProStyleData();
        //}
    }

    /// <summary>
    /// 成品资料画册所需信息
    /// </summary>
    public class StylePictureAlbum : ProBYQ, INotifyPropertyChanged
    {
        private bool _isReadOnly = true;
        /// <summary>
        /// 是否只读
        /// <remarks>否，用于款色搭配管理</remarks>
        /// </summary>
        public bool IsReadOnly { get { return _isReadOnly; } set { _isReadOnly = value; } }

        public string QuarterName { get; set; }

        public string BrandName { get; set; }
        //public string PictureUrl { get; set; }

        private IEnumerable<ProStyleForPictureAlbum> _styles;
        public IEnumerable<ProStyleForPictureAlbum> Styles
        {
            get
            {
                if (_styles == null && this.ID != default(int))
                {
                    var styles = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle>(o => o.BYQID == this.ID);
                    var pictures = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ProSCPicture>();
                    var data = (from style in styles
                                from picture in pictures
                                where style.ID == picture.StyleID
                                select style).Distinct();//new ProStyleForPictureAlbum(style);
                    _styles = data.Select(o => new ProStyleForPictureAlbum(o)).ToList();
                }
                return _styles;
            }
        }

        private ProStyleForPictureAlbum _selectedStyle;
        public ProStyleForPictureAlbum SelectedStyle
        {
            get
            {
                if (_selectedStyle == null && Styles != null && Styles.Count() > 0)
                {
                    _selectedStyle = Styles.ElementAt(0);
                }
                return _selectedStyle;
            }
            set
            {
                if (_selectedStyle != value)
                {
                    _selectedStyle = value;
                    OnPropertyChanged("SelectedStyle");
                }
            }
        }

        //private ImageSource _coverImage;
        public ImageSource CoverImage
        {
            get
            {
                if (SelectedStyle == null || SelectedStyle.SelectedPicture == null)
                    return null;
                else
                    return SelectedStyle.SelectedPicture.Picture;
                //if (_coverImage == null)
                //{
                //    if (Styles == null || Styles.Count() == 0)
                //        _coverImage = ProductHelper.GenerateNullImage();
                //    else
                //    {
                //        var style = Styles.ElementAt(0);
                //        if (style.Colors == null || style.Colors.Count() == 0)
                //            _coverImage = ProductHelper.GenerateNullImage();
                //        else
                //            _coverImage = ProductHelper.GetProductImage(style.Code + style.Colors.First().Code);
                //    }
                //}
                //return _coverImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProStyleForPictureAlbum : ProStyleBO
    {
        private ProSCPictureBO _selectedPicture;
        public ProSCPictureBO SelectedPicture
        {
            get
            {
                if (_selectedPicture == null && Pictures != null)
                {
                    _selectedPicture = Pictures.FirstOrDefault();
                    //if (_selectedPicture == null)
                    //    _selectedPicture = new ProSCPictureBO();
                }
                return _selectedPicture;
            }
            set
            {
                if (value != null && value != _selectedPicture)
                {
                    _selectedPicture = value;
                    OnPropertyChanged("SelectedPicture");
                }
            }
        }

        public ProStyleForPictureAlbum(ProStyle style)
            : base(style)
        { }
    }
}
