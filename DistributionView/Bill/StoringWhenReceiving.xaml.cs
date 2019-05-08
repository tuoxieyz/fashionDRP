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
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls;
using DistributionViewModel;
using DistributionModel;
using DomainLogicEncap;
using View.Extension;
using SysProcessViewModel;
using ERPViewModelBasic;
using SysProcessView;
using ERPModelBO;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for StoringWhenReceiving.xaml
    /// </summary>
    public partial class StoringWhenReceiving : UserControl
    {
        Dictionary<int, BillStoringWhenReceivingVM> _dicDataContext = new Dictionary<int, BillStoringWhenReceivingVM>();

        public StoringWhenReceiving()
        {
            InitializeComponent();

            //Loaded事件会在页签切换时触发，因此会导致侦听方法的反复调用
            //this.Loaded += delegate
            //{
            //    RadGridView1.ItemsSource = ReportDataContext.SearchBillDeliveryForStoring();
            //};
            RadGridView1.ItemsSource = BillStoringWhenReceivingVM.SearchBillDeliveryForStoring();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (DeliverySearchEntity)e.Row.Item;
                var grid = (Grid)e.DetailsElement;
                if (!_dicDataContext.ContainsKey(item.ID))
                {
                    _dicDataContext.Add(item.ID, new BillStoringWhenReceivingVM(item));
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
                SysProcessView.UIHelper.ProductCodeInput<BillStoring, BillStoringDetails, ProductForStoringWhenReceiving>(tb, gvDatas.DataContext as BillStoringWhenReceivingVM, this);
                gvDatas.CalculateAggregates();
                e.Handled = true;//设为true，避免父radgridview获取焦点（默认父radgridview获取焦点并将当前行选择为下一行，文本框将丢失焦点）
            }
        }

        //private void ProductCodeInput(TextBox txtProductCode, RadGridView gv)
        //{
        //    string code = txtProductCode.Text.Trim();
        //    if (!string.IsNullOrEmpty(code))
        //    {
        //        if (gv.HasItems)
        //        {
        //            foreach (var item in gv.Items)
        //            {
        //                var product = (ProductForStoringWhenReceiving)item;
        //                if (product.ProductCode == code)
        //                {
        //                    product.ReceiveQuantity += 1;
        //                    return;
        //                }
        //            }
        //        }
        //        var datas = _billVM.GetProductForBill(code);
        //        if (datas != null && datas.Count > 0)
        //        {
        //            if (datas.Count == 1)
        //            {
        //                //datas[0].ReceiveQuantity = 1;
        //                //gv.Items.Add(datas[0]);
        //                MessageBox.Show("发货单不包含货品:" + datas[0].ProductCode);
        //                return;
        //            }
        //            else
        //            {
        //                ProStyleQuantitySetWin win = new ProStyleQuantitySetWin();
        //                win.DataContext = datas;
        //                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
        //                win.SetCompletedEvent += delegate
        //                {
        //                    foreach (var data in datas)
        //                    {
        //                        if (data.Quantity != 0)
        //                        {
        //                            this.SetProductQuantity(gv, data);
        //                        }
        //                    }
        //                };
        //                win.ShowDialog();
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("没有相关成品信息.");
        //        }
        //    }
        //    txtProductCode.Clear();
        //}

        //private void SetProductQuantity(RadGridView gv, ProductForStoringWhenReceiving p)
        //{
        //    foreach (var item in gv.Items)
        //    {
        //        var product = (ProductForStoringWhenReceiving)item;
        //        if (product.ProductID == p.ProductID)
        //        {
        //            product.ReceiveQuantity += p.Quantity;
        //            return;
        //        }
        //    }
        //    //gv.Items.Add(p);
        //    MessageBox.Show("发货单不包含货品:" + p.ProductCode);
        //}

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
            BillStoringWhenReceivingVM context = grid.DataContext as BillStoringWhenReceivingVM;
            var opresult = context.CheckWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                btn.IsEnabled = true;
                return;
            }
            BillStoring bill = new BillStoring();
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            bill.StorageID = context.StorageID;
            bill.RefrenceBillCode = ((DeliverySearchEntity)grid.Tag).Code;
            bill.BillType = (int)BillTypeEnum.BillDelivery;
            bill.Remark = "收货入库";
            bill.BrandID = ((DeliverySearchEntity)grid.Tag).BrandID;
            context.Master = bill;

            var result = context.Save();
            if (result.IsSucceed)
                MessageBox.Show("保存成功");
            else
            {
                btn.IsEnabled = true;
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }
    }
}
