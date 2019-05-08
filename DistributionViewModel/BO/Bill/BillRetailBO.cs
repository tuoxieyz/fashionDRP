using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;

namespace DistributionViewModel
{
    public class BillRetailBOTemp : BillRetail
    {
        public DateTime CreateDate { get; set; }
    }

    public class ProductForRetail : DistributionProductShow
    {
        /// <summary>
        /// 是否应用了VIP折扣
        /// </summary>
        public bool IsApplyVIPDiscount { get; set; }

        private decimal _cutMoney = 0;
        /// <summary>
        /// 扣减金额
        /// </summary>
        public decimal CutMoney
        {
            get { return _cutMoney; }
            set
            {
                if (_cutMoney != value)
                {
                    _cutMoney = value;
                    OnPropertyChanged("CutMoney");
                }
            }
        }

        public decimal RealMoney
        {
            get { return Price * Quantity * Discount / 100 - CutMoney; ; }
            set
            {
                CutMoney = Price * Quantity * Discount / 100 - value;
            }
        }

        public override decimal Discount
        {
            get
            {
                return base.Discount;
            }
            set
            {
                base.Discount = value;
                OnPropertyChanged("RealMoney");
            }
        }
    }

    /// <summary>
    /// VIP升级信息
    /// </summary>
    public class VIPUpgradeInfo
    {
        public string VIPInfo { get; set; }
        public IEnumerable<VIPUpTacticForCheck> UpTactics { get; set; }
    }

    public class BillRetailDetailsForPrint : BillRetailDetails
    {
        public string ProductCode { get; set; }
        //public decimal OriginalPrice { get; set; }
    }

    public class VIPUpTacticForCheck : VIPUpTactic
    {
        public string BrandName { get; set; }
        public string FormerKindName { get; set; }
        public string AfterKindName { get; set; }
        public bool IsChecked { get; set; }
        public string Description { get { return string.Format("{0}:{1} 升 {2},扣减积分{3}.", BrandName, FormerKindName, AfterKindName, CutPoint); } }
    }
}
