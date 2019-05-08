using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using System.Windows.Input;
using Telerik.Windows.Controls;
using ERPViewModelBasic;

namespace DistributionViewModel
{
    /// <summary>
    /// 发货单查询实体
    /// </summary>
    public class DeliverySearchEntity : BillDelivery, INotifyPropertyChanged
    {
        /// <summary>
        /// 开单人姓名
        /// </summary>
        public string CreatorName { get; set; }

        public DateTime CreateDate { get; set; }

        public string BrandName { get; set; }

        /// <summary>
        /// 下级机构名称
        /// </summary>
        public string ToOrganizationName { get; set; }

        public override int Status
        {
            get
            {
                return base.Status;
            }
            set
            {
                base.Status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("StatusName");
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public string StatusName { get { return Enum.GetName(typeof(BillDeliveryStatusEnum), Status); } }

        /// <summary>
        /// 出货仓库
        /// </summary>
        public string StorageName { get; set; }

        /// <summary>
        /// 发货数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 货品总价(当下级机构收货时为上浮之后的价格)
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// 结算总价
        /// </summary>
        public decimal TotalCostMoney { get; set; }

        public string DeliveryKindName {
            get {
                return DeliveryKind == 0 ? "正常发货" : "折价发货";
            }
        }

        private IEnumerable<DistributionProductShow> _details;
        public IEnumerable<DistributionProductShow> Details
        {
            get
            {
                if (_details == null)
                    _details = ReportDataContext.GetBillDeliveryDetails<DistributionProductShow>(this.ID);
                return _details;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProductForDelivery : DistributionProductShow
    {
        /// <summary>
        /// [下级机构]上浮价
        /// </summary>
        public decimal FloatPrice { get; set; }

        /// <summary>
        /// 未完成订单量
        /// </summary>
        public int OrderQuantity { get; set; }

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
    }


    /// <summary>
    /// 单据状态
    /// </summary>
    public enum BillDeliveryStatusEnum
    {
        在途中 = 0,
        已入库 = 1,
        已装箱未配送 = 2
    }
}
