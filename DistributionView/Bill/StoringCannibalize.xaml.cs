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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using DistributionModel;
using View.Extension;
using SysProcessViewModel;
using ERPViewModelBasic;
using SysProcessView;
using ERPModelBO;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for StoringCannibalize.xaml
    /// </summary>
    public partial class StoringCannibalize : UserControl
    {
        //BillStoringCannibalizeVM _billVM = new BillStoringCannibalizeVM();
        Dictionary<int, BillStoringCannibalizeVM> _dicDataContext = new Dictionary<int, BillStoringCannibalizeVM>();

        public StoringCannibalize()
        {
            InitializeComponent();

            RadGridView1.ItemsSource = BillStoringCannibalizeVM.SearchBillCannibalizeForStoring();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (CannibalizeSearchEntity)e.Row.Item;
                var grid = (Grid)e.DetailsElement;
                if (!_dicDataContext.ContainsKey(item.ID))
                {
                    _dicDataContext.Add(item.ID, new BillStoringCannibalizeVM(item));
                }
                grid.DataContext = _dicDataContext[item.ID];
                grid.Tag = item;
                //var gv = View.Extension.UIHelper.GetVisualChild<RadGridView>(grid);
                //gv.ItemsSource = BillStoringCannibalizeVM.GetBillCannibalizeDetails(item.ID);
                //var cbxStorage = View.Extension.UIHelper.GetVisualChild<DataFormComboBoxField>(grid);
                //cbxStorage.ItemsSource = StorageInfoVM.Storages;
            }
        }

        private void tbProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                var gvDatas = tb.ParentOfType<Grid>().FindChildByType<RadGridView>();
                SysProcessView.UIHelper.ProductCodeInput<BillStoring, BillStoringDetails, ProductForStoringWhenReceiving>(tb, gvDatas.DataContext as BillStoringCannibalizeVM, this);
                gvDatas.CalculateAggregates();
                e.Handled = true;//设为true，避免父radgridview获取焦点（默认父radgridview获取焦点并将当前行选择为下一行，文本框将丢失焦点）
            }
        }

        //private void ProductCodeInput(TextBox txtProductCode, RadGridView gv)
        //{
        //    string code = txtProductCode.Text.Trim();
        //    if (!string.IsNullOrEmpty(code))
        //    {
        //        CannibalizeSearchEntity entity = (CannibalizeSearchEntity)gv.DataContext;
        //        var ssdetails = _dicSnapshotDetails[entity.ID];
        //        var ssdetail = ssdetails.FirstOrDefault(o => o.UniqueCode == code);
        //        if (ssdetail == null)
        //        {
        //            MessageBox.Show("唯一码不匹配,请检查");
        //            return;
        //        }
        //        foreach (var item in gv.Items)
        //        {
        //            var product = (ProductForStoringWhenReceiving)item;
        //            if (product.ProductID == ssdetail.ProductID)
        //            {
        //                if (!product.UniqueCodes.Contains(ssdetail.UniqueCode))
        //                {
        //                    product.UniqueCodes.Add(ssdetail.UniqueCode);
        //                    product.ReceiveQuantity += 1;
        //                }
        //            }
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
        //    MessageBox.Show("调拨单不包含货品:" + p.ProductCode);
        //}

        private void ckScanProducts_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox tb = sender as CheckBox;
            var gvDatas = tb.ParentOfType<Grid>().FindChildByType<RadGridView>();
            //由于这里gvData的RowDetails引用的是动态资源，可能正是由于这个原因，它在RowDetailsVisibilityMode改变时每次都动态创建明细控件
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
            BillStoringCannibalizeVM context = grid.DataContext as BillStoringCannibalizeVM;
            //var cbxStorage = (RadComboBox)View.Extension.UIHelper.GetVisualChild<DataFormComboBoxField>(grid).Content;
            //if (context.StorageID == default(int))
            //{
            //    MessageBox.Show("请选择入库仓库");
            //    btn.IsEnabled = true;
            //    return;
            //}
            ////var gvDatas = View.Extension.UIHelper.GetVisualChild<RadGridView>(grid);
            ////var ckScan = View.Extension.UIHelper.GetVisualChild<CheckBox>(grid);
            //var details = new List<BillStoringDetails>();
            ////IEnumerable<BillSnapshotDetailsWithUniqueCode> ssdetails = null;
            //if (context.IsChecked)
            //{
            //    //var tempssdetails = new List<BillSnapshotDetailsWithUniqueCode>();
            //    foreach (var product in context.GridDataItems)
            //    {
            //        if (product.ReceiveQuantity != 0)
            //        {
            //            //tempssdetails.AddRange(product.UniqueCodes.Select(o => new BillSnapshotDetailsWithUniqueCode
            //            //{
            //            //    UniqueCode = o,
            //            //    ProductID = product.ProductID
            //            //}));
            //            details.Add(new BillStoringDetails { ProductID = product.ProductID, Quantity = product.ReceiveQuantity });
            //        }
            //    }
            //    //ssdetails = tempssdetails;
            //}
            //else
            //{
            //    //ssdetails = _dicSnapshotDetails[((CannibalizeSearchEntity)btn.DataContext).ID];
            //    foreach (var product in context.GridDataItems)
            //    {
            //        if (product.Quantity != 0)
            //        {
            //            details.Add(new BillStoringDetails { ProductID = product.ProductID, Quantity = product.Quantity });
            //        }
            //    }
            //}
            //if (details.Count == 0)
            //{
            //    MessageBox.Show("没有需要保存的数据");
            //    btn.IsEnabled = true;
            //    return;
            //}
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
            bill.RefrenceBillCode = ((CannibalizeSearchEntity)grid.Tag).Code;
            bill.BillType = (int)BillTypeEnum.BillCannibalize;
            bill.Remark = "调拨入库";
            bill.BrandID = ((CannibalizeSearchEntity)grid.Tag).BrandID;
            context.Master = bill;

            opresult = context.Save();
            if (opresult.IsSucceed)
                MessageBox.Show("入库成功");
            else
            {
                btn.IsEnabled = true;
                MessageBox.Show("入库失败\n失败原因:" + opresult.Message);
            }
        }
    }
}
