using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using ERPViewModelBasic;
using DistributionModel;
using SysProcessViewModel;
using System.Collections.ObjectModel;

namespace DistributionViewModel
{
    /// <summary>
    /// 订单查询实体
    /// </summary>
    public class OrderSearchEntity : ViewModelBase
    {
        public int BillID { get; set; }
        public string BillCode { get; set; }
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public string 单据编号 { get; set; }
        public string 开单人 { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime CreateDate { get; set; }

        private int _orderQuatity = 0;

        public int 订货数量
        {
            get { return _orderQuatity; }
            set
            {
                if (_orderQuatity != value)
                {
                    _orderQuatity = value;
                    OnPropertyChanged("订货数量");
                }
            }
        }

        private int _deliverQuatity = 0;

        public int 已发数量
        {
            get { return _deliverQuatity; }
            set
            {
                if (_deliverQuatity != value)
                {
                    _deliverQuatity = value;
                    OnPropertyChanged("已发数量");
                }
            }
        }

        private string _deliverState = null;

        public string 发货状态
        {
            get { return _deliverState; }
            set
            {
                if (_deliverState != value)
                {
                    _deliverState = value;
                    OnPropertyChanged("发货状态");
                }
            }
        }

        private bool _isValid = true;
        /// <summary>
        /// 有效(true)or已作废(false)
        /// </summary>
        public bool 订单状态
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged("订单状态");
                }
            }
        }
        public string 备注 { get; set; }
        public string 机构名称 { get; set; }
        public int OrganizationID { get; set; }

        private int _cancelQuantity = 0;

        public int 取消量
        {
            get { return _cancelQuantity; }
            set
            {
                if (_cancelQuantity != value)
                {
                    _cancelQuantity = value;
                    OnPropertyChanged("取消量");
                }
            }
        }

        private IEnumerable<ProductForOrderReport> _details;
        public IEnumerable<ProductForOrderReport> Details
        {
            get
            {
                if (_details == null)
                {
                    _details = ReportDataContext.GetBillOrderDetails(this.BillID);
                }
                return _details;
            }
        }

        private ObservableCollection<BillOrderChange> _changes;
        public ObservableCollection<BillOrderChange> Changes
        {
            get
            {
                if (_changes == null)
                {
                    var lp = VMGlobal.DistributionQuery.LinqOP;
                    _changes = new ObservableCollection<BillOrderChange>(lp.Search<BillOrderChange>(o => o.BillID == this.BillID).OrderByDescending(o => o.CreateTime));
                }
                return _changes;
            }
        }
    }

    /// <summary>
    /// 用于订单汇总报表实体
    /// </summary>
    public class OrderEntityForAggregation : BillEntityForAggregation
    {
        public int QuaDelivered { get; set; }
    }

    /// <summary>
    /// 订单汇总报表实体
    /// </summary>
    public class OrderAggregationEntity : DistributionProductShow
    {
        public int QuaDelivered { get; set; }

        private int _quaStock = 0;//100;
        public int QuaStock
        {
            get { return _quaStock; }
            set
            {
                if (_quaStock != value)
                {
                    _quaStock = value;
                    OnPropertyChanged("QuaStock");
                }
            }
        }

        private int _quaAvailableStock = 0;//100;
        public int QuaAvailableStock
        {
            get { return _quaAvailableStock; }
            set
            {
                if (_quaAvailableStock != value)
                {
                    _quaAvailableStock = value;
                    OnPropertyChanged("QuaAvailableStock");
                }
            }
        }
    }

    public class OrderDistributionEntity : OrderAggregationEntity
    {
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; }
    }
}
