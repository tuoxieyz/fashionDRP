using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;

namespace DistributionViewModel
{
    public class DistributionProductShow : ProductShow
    {
        private decimal _discount = 100;
        public virtual decimal Discount
        {
            get { return _discount; }
            set
            {
                _discount = value;
                OnPropertyChanged("Discount");
            }
        }

        /// <summary>
        /// 折后价小计
        /// </summary>
        public decimal SettlementPrice { get {
            return Discount * Price * Quantity / 100;
        } }

        //public string Year { get; set; }
        //public int Quarter { get; set; }
    }

    /// <summary>
    /// 分布实体
    /// </summary>
    public class DistributionEntity : DistributionProductShow
    {
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; }
    }
}
