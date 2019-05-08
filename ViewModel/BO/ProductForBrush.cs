using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using SysProcessViewModel;

namespace ERPViewModelBasic
{
    public class ProductShow : ViewModelBase
    {
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public int BrandID { get; set; }//鉴于BrandID经常作为界面上列表间的关联和查询条件，还是保留着吧
        public string BrandCode { get; set; }
        public int StyleID { get; set; }
        public int ColorID { get; set; }
        public string StyleCode { get; set; }
        public string ColorCode { get; set; }
        public string ColorName { get; set; }
        public int SizeID { get; set; }
        public string SizeCode { get; set; }
        public string SizeName { get; set; }
        public int BYQID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public virtual decimal Price { get; set; }
        public int NameID { get; set; }
        public string ProductName { get; set; }

        private int _quantity = 0;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

#if UniqueCode
        private ObservableCollection<string> _uniqueCodes;
        /// <summary>
        /// 唯一码集合
        /// </summary>
        public ObservableCollection<string> UniqueCodes
        {
            get
            {
                if (_uniqueCodes == null)
                    _uniqueCodes = new ObservableCollection<string>();
                return _uniqueCodes;
            }
        }
#endif

        //private ImageSource _picture;
        //public ImageSource Picture
        //{
        //    get
        //    {
        //        if (_picture == null)
        //        {
        //            _picture = ProductHelper.GetProductImage(StyleID, ColorID);
        //        }
        //        return _picture;
        //    }
        //}
    }
}
