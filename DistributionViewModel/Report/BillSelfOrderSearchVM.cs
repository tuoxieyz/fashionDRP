using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillSelfOrderSearchVM : BillPagedReportVM<OrderSearchEntity>
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "订货品牌", PropertyName = "BrandID", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("CreateDate", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateDate", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        /// <summary>
        /// 查询本级订单
        /// </summary>
        protected override IEnumerable<OrderSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var orderContext = lp.GetDataContext<BillOrder>();
            var userContext = lp.GetDataContext<ViewUser>();

            var billData = from order in orderContext
                           from user in userContext
                           where order.CreatorID == user.ID && order.OrganizationID == VMGlobal.CurrentUser.OrganizationID && order.IsDeleted == false
                           select new OrderSearchEntity
                           {
                               BillID = order.ID,
                               BrandID = order.BrandID,
                               单据编号 = order.Code,
                               CreateDate = order.CreateTime.Date,
                               CreateTime = order.CreateTime,
                               开单人 = user.Name,
                               备注 = order.Remark,
                               订单状态 = !order.IsDeleted
                           };
            int totalCount = 0;
            var result = ReportDataContext.SearchOrder(billData, FilterDescriptors, DetailsDescriptors, PageIndex, PageSize, ref totalCount);
            TotalCount = totalCount;
            return result;
        }
    }

    public class ProductForOrderReport : DistributionProductShow
    {
        /// <summary>
        /// 订单明细单条记录ID
        /// </summary>
        public int DetailID { get; set; }

        private int _quaCancel = 0;

        /// <summary>
        /// 取消量
        /// </summary>
        public int QuaCancel
        {
            get { return _quaCancel; }
            set
            {
                if (value < 0 || value > Quantity - QuaDelivered)
                    return;
                if (_quaCancel != value)
                {
                    _quaCancel = value;
                    OnPropertyChanged("QuaCancel");
                    OnPropertyChanged("Status");
                }
            }
        }
        /// <summary>
        /// 已发货数量
        /// </summary>
        public int QuaDelivered { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string Status
        {
            get
            {
                var realQua = Quantity - QuaCancel;
                var status = realQua == QuaDelivered ? "已完成" : (QuaDelivered == 0 ? "未发货" : ((realQua > QuaDelivered ? "部分已发货" : "数据有误")));
                return status;
            }
        }
    }
}
