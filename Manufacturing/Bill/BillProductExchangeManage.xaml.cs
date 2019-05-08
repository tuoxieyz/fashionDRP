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
using Manufacturing.ViewModel;
using System.Globalization;
using Telerik.Windows.Controls;
using ERPViewModelBasic;
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for BillProductExchangeManage.xaml
    /// </summary>
    public partial class BillProductExchangeManage : UserControl
    {
        BillProductExchangeManageVM _dataContext = new BillProductExchangeManageVM();

        public BillProductExchangeManage()
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
                var item = (BillProductExchangeSearchEntity)e.Row.Item;
                gv.ItemsSource = item.Details;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillProductExchangeSearchEntity entity = (BillProductExchangeSearchEntity)btn.DataContext;
            var result = _dataContext.DeleteBill(entity);
            if (result.IsSucceed)
                entity.IsDeleted = true;
            MessageBox.Show(result.Message);
        }

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillProductExchangeSearchEntity entity = (BillProductExchangeSearchEntity)btn.DataContext;
            var result = _dataContext.RevertBill(entity);
            if (result.IsSucceed)
                entity.IsDeleted = false;
            MessageBox.Show(result.Message);
        }

        private void btnReSend_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillProductExchangeSearchEntity entity = (BillProductExchangeSearchEntity)btn.DataContext;
            var result = _dataContext.ReSendBill(entity);
            if (result.IsSucceed)
                entity.Status = (int)BillProductExchangeStatusEnum.在途中;
            MessageBox.Show(result.Message);
        }

        private void gvDetails_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            var row = e.Cell.ParentRow;
            var item = (ProductForProductExchange)row.DataContext;
            //在有编辑器模板的情况下，e.OldData，e.NewData不准确
            var editor = e.EditingElement as RadNumericUpDown;
            if (editor != null)
            {
                var nowValue = (int)editor.Value;
                item.Quantity = nowValue;

                var result = _dataContext.UpdateDetails(item);
                if (result.IsSucceed)
                {
                    //更新主表状态
                    var parentRow = row.GridViewDataControl.ParentRow;
                    var subcontract = (BillProductExchangeSearchEntity)parentRow.DataContext;
                    this.SetQuantityForOrderEntity(subcontract, parentRow);
                }
                else
                    MessageBox.Show(result.Message);
            }
        }

        private void gvDetails_BeginningEdit(object sender, GridViewBeginningEditRoutedEventArgs e)
        {
            var parentRow = e.Row.GridViewDataControl.ParentRow;
            var subcontract = (BillProductExchangeSearchEntity)parentRow.DataContext;
            if (subcontract.IsDeleted)
            {
                MessageBox.Show("已作废单据不能修改!");
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 更新UI主表[和子表]状态
        /// </summary>
        private void SetQuantityForOrderEntity(BillProductExchangeSearchEntity entity, GridViewRow row)
        {
            _dataContext.SetQuantityForBillEntity(entity);
            RadGridView1.CalculateAggregates();
        }
    }

    internal class BillProductExchangeRowStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is BillProductExchangeSearchEntity)
            {
                BillProductExchangeSearchEntity p = item as BillProductExchangeSearchEntity;
                if (p.Status == (int)BillProductExchangeStatusEnum.被退回)
                {
                    return BillProductExchangeSendBackStyle;
                }
            }
            return null;
        }

        public Style BillProductExchangeSendBackStyle { get; set; }
    }

    public class PEStatusDeleteButtonVisibleCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isDeleted = (bool)values[0];
                int status = (int)values[1];
                if (!isDeleted && status != (int)BillProductExchangeStatusEnum.已入库)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PEStatusSendButtonVisibleCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isDeleted = (bool)values[0];
                int status = (int)values[1];
                if (!isDeleted && status == (int)BillProductExchangeStatusEnum.被退回)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
