using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using DistributionViewModel;
using DomainLogicEncap;
using Telerik.Windows.Controls;
using System.Linq.Expressions;
using Telerik.Windows.Controls.GridView;
using System.Collections;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrderManage.xaml
    /// </summary>
    public partial class OrderManage : UserControl
    {
        //private FloatPriceHelper _fpHelper;
        OrderManageVM _dataContext = new OrderManageVM();

        public OrderManage()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            var childOrgs = VMGlobal.ChildOrganizations;
            if (childOrgs == null || childOrgs.Count == 0)
            {
                oselector.Visibility = Visibility.Collapsed;
            }

            //RadGridView1.LayoutUpdated += new EventHandler(RadGridView1_LayoutUpdated);

            //this.Loaded += delegate { this.btnSearch_Click(null, null); };
        }

        //private bool _isCustomLayouting = false;

        //void RadGridView1_LayoutUpdated(object sender, EventArgs e)
        //{
        //    if (!_isCustomLayouting)
        //    {
        //        _isCustomLayouting = true;
        //        var dataSource = (IList)RadGridView1.ItemsSource;
        //        if (dataSource != null)
        //        {
        //            double width = 0;
        //            foreach (var data in dataSource)
        //            {
        //                var row = RadGridView1.ItemContainerGenerator.ContainerFromItem(data) as GridViewRow;
        //                if (row != null)
        //                {
        //                    var spOperate = row.Cells[row.Cells.Count - 1].Content as StackPanel;//(StackPanel)colOperate.CellTemplate.FindName("spOperate", (FrameworkElement)row.Cells[row.Cells.Count - 1].Content);
        //                    if (spOperate != null)
        //                        width = Math.Max(width, spOperate.ActualWidth);
        //                }
        //            }
        //            colOperate.Width = width + 10;
        //        }
        //        _isCustomLayouting = false;
        //    }
        //}

        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    int totalCount = 0;
        //    //这种方式绑定数据并不会触发RadGridView1的DataContextChanged事件
        //    //RadGridView1.ItemsSource = ReportDataContext.SearchBillOrder(billFilter.FilterDescriptors, detailsFilter.FilterDescriptors);            
        //    RadGridView1.ItemsSource = ReportDataContext.SearchBillOrder(billFilter.FilterDescriptors, detailsFilter.FilterDescriptors, pager.PageIndex, pager.PageSize, ref totalCount);
        //    pager.ItemCount = totalCount;
        //}

        //private void RadDataPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
        //{
        //    //加这个判断是因为不加这个判断会导致列表第一次加载数据时最后的按钮列超出到界面外从而出现滚动条
        //    //因此我把第一次的查询放到整个界面Loaded事件中
        //    //出现这个问题的原因估计是某些列我给它指定了Width，而不是让它们自己根据内容自适应宽度，不过先就这样吧
        //    if (e.OldPageIndex != -1)
        //        this.btnSearch_Click(null, null);
        //}

        //private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        //{
        //    if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
        //    {
        //        var gv = (RadGridView)e.DetailsElement;
        //        //Expression<Func<ProductForOrderReport, string>> expression = o => o.Quantity == o.QuaDelivered ? "已完成" : (o.QuaDelivered == 0 ? "未发货" : ((o.Quantity > o.QuaDelivered ? "部分已发货" : "数据有误")));
        //        //GridViewExpressionColumn column = gv.Columns["Status"] as GridViewExpressionColumn;
        //        //column.Expression = expression;
        //        var item = (OrderSearchEntity)e.Row.Item;
        //        var details = ReportDataContext.GetBillOrderDetails(item.BillID);
        //        if (_fpHelper == null)
        //            _fpHelper = new FloatPriceHelper();
        //        foreach (var d in details)
        //        {
        //            d.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, d.BrandID, d.Year, d.Quarter, d.Price);
        //        }
        //        gv.ItemsSource = details;
        //    }
        //}

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            OrderSearchEntity entity = (OrderSearchEntity)btn.DataContext;
            var result = _dataContext.DeleteOrder(entity);
            if (result.IsSucceed)
                entity.订单状态 = false;
            MessageBox.Show(result.Message);
        }

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            OrderSearchEntity entity = (OrderSearchEntity)btn.DataContext;
            var result = _dataContext.RevertOrder(entity);
            if (result.IsSucceed)
                entity.订单状态 = true;
            MessageBox.Show(result.Message);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            OrderSearchEntity entity = (OrderSearchEntity)btn.DataContext;
            var result = _dataContext.CancelLeftOrderQuantity(entity);
            if (result.IsSucceed)
                this.SetQuantityForOrderEntity(entity, View.Extension.UIHelper.GetAncestor<GridViewRow>(btn));
            MessageBox.Show(result.Message);
        }

        private void btnCancelZero_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            OrderSearchEntity entity = (OrderSearchEntity)btn.DataContext;
            var result = _dataContext.ZeroCancelOrderQuantity(entity);
            if (result.IsSucceed)
            {
                this.SetQuantityForOrderEntity(entity, View.Extension.UIHelper.GetAncestor<GridViewRow>(btn));
            }
            MessageBox.Show(result.Message);
        }

        /// <summary>
        /// 更新UI主表[和子表]状态
        /// </summary>
        private void SetQuantityForOrderEntity(OrderSearchEntity entity, GridViewRow row, bool refreshDetails = true)
        {
            _dataContext.SetQuantityForOrderEntity(entity);
            //if (refreshDetails && row.DetailsVisibility != null && row.DetailsVisibility == Visibility.Visible)
            //{
            //    row.DetailsVisibility = Visibility.Collapsed;
            //    row.DetailsVisibility = Visibility.Visible;
            //}
            RadGridView1.CalculateAggregates();
        }

        private void gvDetails_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            //在有编辑器模板的情况下，e.OldData，e.NewData不准确
            var editor = (RadNumericUpDown)e.EditingElement;
            var nowValue = (int)editor.Value;

            var row = e.Cell.ParentRow;
            var item = (ProductForOrderReport)row.DataContext;

            //更新主表状态
            var parentRow = row.GridViewDataControl.ParentRow;
            var order = (OrderSearchEntity)parentRow.DataContext;
            var result = _dataContext.UpdateDetailsCancelQuantity(item.DetailID, nowValue, item.ProductCode, order);
            if (result.IsSucceed)
            {
                item.QuaCancel = nowValue;
                this.SetQuantityForOrderEntity(order, parentRow, false);
            }
            else
                MessageBox.Show(result.Message);
        }

        private void gvDetails_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
        {
            var parentRow = e.Row.GridViewDataControl.ParentRow;
            var order = (OrderSearchEntity)parentRow.DataContext;
            if (!order.订单状态)
            {
                MessageBox.Show("已作废订单不能修改!");
                e.Cancel = true;
            }
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
