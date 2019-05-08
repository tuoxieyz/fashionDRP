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
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using DistributionViewModel;
using DistributionModel;
using ERPViewModelBasic;
using SysProcessViewModel;
using System.Linq.Expressions;
using System.Data;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillSubordinateGoodReturnSearch.xaml
    /// </summary>
    public partial class BillSubordinateGoodReturnSearch : UserControl
    {
        Expression<Func<DataRow, decimal>> _expressionPD = prod => (decimal)prod["Price"] * (decimal)prod["Discount"] / 100;
        Expression<Func<DataRow, decimal>> _expressionPDQ = prod => (decimal)prod["Price"] * (decimal)prod["Discount"] * (int)prod["Quantity"] / 100;

        public BillSubordinateGoodReturnSearch()
        {
            this.DataContext = new BillSubordinateGoodReturnSearchVM();
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (BillGoodReturnForSearch)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                if (gv.Tag == null)
                {
                    gv.Tag = new object();
                    SysProcessView.UIHelper.TransferSizeToHorizontal(gv);
                    GridViewExpressionColumn expColumn = gv.Columns["colDiscountPrice"] as GridViewExpressionColumn;
                    expColumn.Expression = _expressionPD;
                    expColumn = gv.Columns["colDiscountPriceQuantity"] as GridViewExpressionColumn;
                    expColumn.Expression = _expressionPDQ;
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
            SysProcessView.UIHelper.BillExportExcel("下级退货单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillGoodReturnForSearch)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("下级退货单", RadGridView1, item);
        }
    }
}
