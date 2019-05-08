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
using Telerik.Windows.Controls;
using DistributionModel;
using SysProcessViewModel;
using System.Data;
using System.Linq.Expressions;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for RetailAggregation.xaml
    /// </summary>
    public partial class RetailAggregation : UserControl
    {
        public RetailAggregation()
        {
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            GridViewExpressionColumn expColumn = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            expColumn.Expression = expression;
            Expression<Func<DataRow, decimal>> expDiscount = prod => (decimal)prod["CostMoney"] / ((decimal)prod["Price"] * (int)prod["Quantity"]);
            GridViewExpressionColumn colDiscount = RadGridView1.Columns["colDiscount"] as GridViewExpressionColumn;
            colDiscount.Expression = expDiscount;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
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
                    case "ShiftID":
                        cbx.ItemsSource = VMGlobal.DistributionQuery.LinqOP.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
