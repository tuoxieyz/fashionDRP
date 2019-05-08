using DistributionViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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

namespace DistributionView.Reports
{
    /// <summary>
    /// ShopProfit.xaml 的交互逻辑
    /// </summary>
    public partial class ShopProfit : UserControl
    {
        ShopProfitVM _dataContext = new ShopProfitVM();

        public ShopProfit()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            this.TransferExpenseToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["SaleMoney"] / (int)prod["SaleQuantity"];
            GridViewExpressionColumn expColumn = RadGridView1.Columns["colAverage"] as GridViewExpressionColumn;
            expColumn.Expression = expression;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void TransferExpenseToHorizontal(RadGridView gv)
        {
            System.ComponentModel.TypeDescriptor.GetProperties(gv)["ItemsSource"].AddValueChanged(gv, new EventHandler(gv_ItemsSourceChanged));
            var ExpenseCol = gv.Columns.FirstOrDefault<Telerik.Windows.Controls.GridViewColumn>(o => o.Header.ToString() == "费用类别");
            if (ExpenseCol != null)
            {
                int index = gv.Columns.IndexOf(ExpenseCol);
                gv.Columns.RemoveAt(index);
                foreach (var kind in _dataContext.ExpenseKinds)
                {
                    var col = new GridViewDataColumn() { Header = kind.Name, DataMemberBinding = new Binding(kind.Name), Tag = "ExpenseCol", IsGroupable = false, DataFormatString = "{0:C2}" };//, Name = size.Name
                    gv.Columns.Insert(index, col);
                    index++;
                }
            }
        }

        private void gv_ItemsSourceChanged(object sender, EventArgs e)
        {
            RadGridView gv = sender as RadGridView;
            var dt = gv.ItemsSource as DataTable;
            if (dt != null)
            {
                var ExpenseCols = gv.Columns.Where<Telerik.Windows.Controls.GridViewColumn>(o => o.Tag == "ExpenseCol").ToArray();
                foreach (var col in ExpenseCols)
                {
                    if (dt.Columns.Contains(col.Header.ToString()))
                        col.IsVisible = true;
                    else
                        col.IsVisible = false;
                }
            }
        }
    }
}
