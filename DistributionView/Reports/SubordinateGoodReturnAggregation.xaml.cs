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
using SysProcessViewModel;
using System.Linq.Expressions;
using System.Data;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SubordinateGoodReturnAggregation.xaml
    /// </summary>
    public partial class SubordinateGoodReturnAggregation : UserControl
    {
        public SubordinateGoodReturnAggregation()
        {
            this.DataContext = new SubordinateGoodReturnAggregationVM();
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            GridViewExpressionColumn expColumn = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            expColumn.Expression = expression;
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
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                    case "NameID":
                        cbx.ItemsSource = VMGlobal.ProNames;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
