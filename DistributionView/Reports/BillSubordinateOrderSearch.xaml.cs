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
using DomainLogicEncap;
using DistributionViewModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;
using ERPViewModelBasic;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillSubordinateOrderSearch.xaml
    /// </summary>
    public partial class BillSubordinateOrderSearch : UserControl
    {
        //private FloatPriceHelper _fpHelper;

        public BillSubordinateOrderSearch()
        {
            //var childOrgs = OrganizationLogic.GetChildOrganizations(VMGlobal.CurrentUser.OrganizationID);
            //if (childOrgs == null || childOrgs.Count == 0)
            //{
            //    MessageBox.Show("没有下级机构，请选择其它菜单查询");
            //    return;
            //}
            InitializeComponent();
        }

        //private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        //{
        //    if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
        //    {
        //        var gv = (RadGridView)e.DetailsElement;
        //        var item = e.Row.Item;
        //        var proBill = item.GetType().GetProperty("BillID");
        //        var billID = (int)proBill.GetValue(item, null);
        //        var details = ReportDataContext.GetBillOrderDetails(billID);
        //        //if (_fpHelper == null)
        //        //    _fpHelper = new FloatPriceHelper();
        //        //foreach (var d in details)
        //        //{
        //        //    d.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, d.ProductID);
        //        //}
        //        gv.ItemsSource = details;
        //    }
        //}

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (OrderSearchEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                if (gv.Tag == null)
                {
                    gv.Tag = new object();
                    SysProcessView.UIHelper.TransferSizeToHorizontal(gv);
                }
                gv.ItemsSource = new BillReportHelper().TransferSizeToHorizontal<ProductForOrderReport>(item.Details);
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

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (OrderSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("下级订货单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (OrderSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("下级订货单", RadGridView1, item);
        }
    }
}
