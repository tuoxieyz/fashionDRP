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
using System.Globalization;
using Telerik.Windows.Controls;
using Manufacturing.ViewModel;
using Telerik.Windows.Controls.GridView;
using ERPViewModelBasic;
using View.Extension;
using SysProcessViewModel;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for BillSubcontractManage.xaml
    /// </summary>
    public partial class BillSubcontractManage : UserControl
    {
        BillSubcontractManageVM _dataContext = new BillSubcontractManageVM();

        public BillSubcontractManage()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbx = e.Editor as RadComboBox;
                cbx.ItemsSource = VMGlobal.PoweredBrands;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (BillSubcontractSearchEntity)e.Row.Item;
                gv.ItemsSource = item.Details;
                foreach (var d in item.Details)
                {
                    if (d.DeliveryDate < DateTime.Now.Date && d.Status != "已完成")//过期未完成
                    {
                        var row = gv.ItemContainerGenerator.ContainerFromItem(d) as GridViewRow;
                        UIHelper.SetGridRowValidBackground(row, false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI主表[和子表]状态
        /// </summary>
        private void SetQuantityForOrderEntity(BillSubcontractSearchEntity entity, GridViewRow row, bool refreshDetails = true)
        {
            _dataContext.SetQuantityForBillEntity(entity);
            if (refreshDetails && row.DetailsVisibility != null && row.DetailsVisibility == Visibility.Visible)
            {
                row.DetailsVisibility = Visibility.Collapsed;
                row.DetailsVisibility = Visibility.Visible;
            }
            RadGridView1.CalculateAggregates();
        }

        private void gvDetails_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            var row = e.Cell.ParentRow;
            var item = (ProductForProduceBrush)row.DataContext;
            //在有编辑器模板的情况下，e.OldData，e.NewData不准确
            if (e.EditingElement is RadNumericUpDown || e.EditingElement is RadDatePicker)
            {
                var editor = e.EditingElement as RadNumericUpDown;
                if (editor != null)
                {
                    var nowValue = (int)editor.Value;
                    var tag = editor.Tag.ToString();
                    if (tag == "QuaCancel") item.QuaCancel = nowValue;
                    if (tag == "QuaCompleted") item.QuaCompleted = nowValue;
                }
                else
                {
                    RadDatePicker picker = e.EditingElement as RadDatePicker;
                    if (picker != null && picker.SelectedValue != null)//若选择的日期非法则为null
                    {
                        item.DeliveryDate = picker.SelectedValue.Value;
                    }
                    else
                    {
                        return;
                    }
                }
                var result = _dataContext.UpdateDetails(item);
                if (result.IsSucceed)
                {
                    //更新主表状态
                    var parentRow = row.GridViewDataControl.ParentRow;
                    var subcontract = (BillSubcontractSearchEntity)parentRow.DataContext;
                    this.SetQuantityForOrderEntity(subcontract, parentRow, false);
                }
                else
                    MessageBox.Show(result.Message);
            }
        }

        private void gvDetails_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
        {
            var parentRow = e.Row.GridViewDataControl.ParentRow;
            var subcontract = (BillSubcontractSearchEntity)parentRow.DataContext;
            if (subcontract.IsDeleted)
            {
                MessageBox.Show("已作废单据不能修改!");
                e.Cancel = true;
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillSubcontractSearchEntity entity = (BillSubcontractSearchEntity)btn.DataContext;
            var result = _dataContext.DeleteBill(entity);
            if (result.IsSucceed)
                entity.IsDeleted = true;
            MessageBox.Show(result.Message);
        }

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillSubcontractSearchEntity entity = (BillSubcontractSearchEntity)btn.DataContext;
            var result = _dataContext.RevertBill(entity);
            if (result.IsSucceed)
                entity.IsDeleted = false;
            MessageBox.Show(result.Message);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillSubcontractSearchEntity entity = (BillSubcontractSearchEntity)btn.DataContext;
            var result = _dataContext.CancelLeftSubcontractQuantity(entity);
            if (result.IsSucceed)
                this.SetQuantityForOrderEntity(entity, View.Extension.UIHelper.GetAncestor<GridViewRow>(btn));
            MessageBox.Show(result.Message);
        }

        private void btnCancelZero_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillSubcontractSearchEntity entity = (BillSubcontractSearchEntity)btn.DataContext;
            var result = _dataContext.ZeroCancelSubcontractQuantity(entity);
            if (result.IsSucceed)
                this.SetQuantityForOrderEntity(entity, View.Extension.UIHelper.GetAncestor<GridViewRow>(btn));
            MessageBox.Show(result.Message);
        }
    }

    #region 转换器

    //public class RevertButtonVisibleWithSubcontractStateCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string isDeletedName = value.ToString();//单据状态
    //        return isDeletedName == "已作废" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class ButtonVisibleWithSubcontractStateCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string isDeletedName = values[0].ToString();//单据状态
                string statusName = values[1].ToString();//生产状态
                string btnName = parameter.ToString();//按钮名称
                switch (btnName)
                {
                    case "整单作废":
                        if (isDeletedName == "有效" && statusName == "未交货")
                            return Visibility.Visible;
                        break;
                    case "取消未完成数量":
                        if (isDeletedName == "有效" && statusName == "部分已交货")
                            return Visibility.Visible;
                        break;
                }
                return Visibility.Hidden;
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ZeroButtonVisibleWithSubcontractStateCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string isDeletedName = values[0].ToString();//单据状态
                int cancelQua = (int)values[1];
                if (isDeletedName == "有效" && cancelQua > 0)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    ///// <summary>
    ///// 最大可以取消量=下单量-已交付数量
    ///// </summary>
    //public class SubcontractMaxCancelQuantityCvt : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        try
    //        {
    //            int orderQua = (int)values[0];
    //            int deliverQua = (int)values[1];
    //            return orderQua - deliverQua;
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    #endregion
}
