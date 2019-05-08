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
using DistributionViewModel;
using DistributionModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using View.Extension;
using ERPViewModelBasic;
using SysProcessViewModel;
using SysProcessView;
using System.Collections.ObjectModel;
using ERPModelBO;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for StoringReturnGood.xaml
    /// </summary>
    public partial class StoringReturnGood : UserControl
    {
        Dictionary<int, BillStoringReturnGoodVM> _dicDataContext = new Dictionary<int, BillStoringReturnGoodVM>();

        public StoringReturnGood()
        {
            InitializeComponent();
            RadGridView1.ItemsSource = BillStoringReturnGoodVM.SearchBillSubordinateGoodReturnForStoring();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (BillGoodReturnForSearch)e.Row.Item;
                var grid = (Grid)e.DetailsElement;

                if (!_dicDataContext.ContainsKey(item.ID))
                {
                    _dicDataContext.Add(item.ID, new BillStoringReturnGoodVM(item));
                }
                grid.DataContext = _dicDataContext[item.ID];
                grid.Tag = item;
            }
        }

        private void tbProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                var gvDatas = tb.ParentOfType<Grid>().FindChildByType<RadGridView>();
                SysProcessView.UIHelper.ProductCodeInput<BillStoring, BillStoringDetails, ProductForStoringWhenReceiving>(tb, gvDatas.DataContext as BillStoringReturnGoodVM, this);
                gvDatas.CalculateAggregates();
                e.Handled = true;//设为true，避免父radgridview获取焦点（默认父radgridview获取焦点并将当前行选择为下一行，文本框将丢失焦点）
            }
        }

        private void ckScanProducts_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox tb = sender as CheckBox;
            var gvDatas = tb.ParentOfType<Grid>().FindChildByType<RadGridView>();
            gvDatas.RowDetailsVisibilityMode = GridViewRowDetailsVisibilityMode.Collapsed;
            gvDatas.RowStyleSelector = null;
        }

        private void ckScanProducts_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox tb = sender as CheckBox;
            var gvDatas = tb.ParentOfType<Grid>().FindChildByType<RadGridView>();
            gvDatas.RowDetailsVisibilityMode = GridViewRowDetailsVisibilityMode.Visible;
            if (gvDatas.RowStyleSelector == null)
            {
                StoringWhenReceiveRowStyleSelector styleSelector = (StoringWhenReceiveRowStyleSelector)gridLayout.Resources["storingWhenReceiveRowStyleSelector"];
                gvDatas.RowStyleSelector = styleSelector;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadButton;
            btn.IsEnabled = false;

            var grid = btn.GetVisualParent<Grid>();
            BillStoringReturnGoodVM context = grid.DataContext as BillStoringReturnGoodVM;
            var opresult = context.CheckWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                btn.IsEnabled = true;
                return;
            }
            //SetGoodReturnMoneyWin win = new SetGoodReturnMoneyWin();
            //win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            //win.ReturnMoneySettedEvent += money =>
            //{
            //    _billVM.ReturnMoney = money;
            BillStoring bill = new BillStoring();
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            bill.StorageID = context.StorageID;
            bill.RefrenceBillCode = ((BillGoodReturnForSearch)grid.Tag).Code;
            bill.BillType = (int)BillTypeEnum.BillGoodReturn;
            bill.Remark = "退货入库";
            bill.BrandID = ((BillGoodReturnForSearch)grid.Tag).BrandID;
            context.Master = bill;

            opresult = context.Save();
            if (opresult.IsSucceed)
                MessageBox.Show("保存成功");
            else
            {
                btn.IsEnabled = true;
                MessageBox.Show("保存失败\n失败原因:" + opresult.Message);
            }
            //};
            //var flag = win.ShowDialog();
            //if (flag == null || !flag.Value)
            //    btn.IsEnabled = true;
        }

        private void btnSendBack_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            BillGoodReturnForSearch entity = (BillGoodReturnForSearch)btn.DataContext;
            var result = BillStoringReturnGoodVM.SendBack(entity);
            MessageBox.Show(result.Message);
            if (result.IsSucceed)
            {
                var data = RadGridView1.ItemsSource as ObservableCollection<BillGoodReturnForSearch>;
                data.Remove(entity);
            }
        }
    }
}
