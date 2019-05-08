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
using Telerik.Windows.Controls;
using DistributionViewModel;
using Telerik.Windows.Controls.GridView;
using DistributionModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillSelfGoodReturnSearch.xaml
    /// </summary>
    public partial class BillSelfGoodReturnSearch : UserControl
    {
        public BillSelfGoodReturnSearch()
        {
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (BillGoodReturnForSearch)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = ReportDataContext.SearchBillDetails<BillGoodReturnDetails>(item.ID);
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
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillGoodReturnForSearch)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("退货单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillGoodReturnForSearch)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("退货单", RadGridView1, item);
        }
    }
}
