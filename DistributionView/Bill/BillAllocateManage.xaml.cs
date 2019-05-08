using DistributionViewModel;
using ERPViewModelBasic;
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
using Telerik.Windows.Controls;

namespace DistributionView.Bill
{
    /// <summary>
    /// BillAllocateManage.xaml 的交互逻辑
    /// </summary>
    public partial class BillAllocateManage : UserControl
    {
        BillAllocateManageVM _dataContext = new BillAllocateManageVM();

        public BillAllocateManage()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (AllocateSearchEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                if (gv.Tag == null)
                {
                    gv.Tag = new object();
                    SysProcessView.UIHelper.TransferSizeToHorizontal(gv);
                }
                gv.ItemsSource = new BillReportHelper().TransferSizeToHorizontal<DistributionProductShow>(item.Details);
            }
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "StorageID":
                        cbx.ItemsSource = StorageInfoVM.Storages;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (AllocateSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("配货单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (AllocateSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("配货单", RadGridView1, item);
        }

        private void btnHandle_Click(object sender, RoutedEventArgs e)
        {
            var item = (AllocateSearchEntity)((RadButton)sender).DataContext;
            var result = _dataContext.Handle(item);
            MessageBox.Show(result.Message);
        }
    }
}
