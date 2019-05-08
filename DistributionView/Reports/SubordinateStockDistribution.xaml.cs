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
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using System.Data;
using telerik = Telerik.Windows.Controls;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SubordinateStockDistribution.xaml
    /// </summary>
    public partial class SubordinateStockDistribution : UserControl
    {
        SubordinateStockDistributionVM _dataContext = new SubordinateStockDistributionVM();

        public SubordinateStockDistribution()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RadGridView1.ItemsSource = null;//将数据源设为null，否则在下面更改列表结构时将报错
            while (RadGridView1.Columns.Count > 5)
                RadGridView1.Columns.RemoveAt(5);
            var data = _dataContext.Search();
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("ProductCode", typeof(string)));
            table.Columns.Add(new DataColumn("BrandCode", typeof(string)));
            table.Columns.Add(new DataColumn("StyleCode", typeof(string)));
            table.Columns.Add(new DataColumn("ColorCode", typeof(string)));
            table.Columns.Add(new DataColumn("SizeName", typeof(string)));
            var onames = data.Select(o => o.OrganizationName).Distinct().ToList();
            onames.Add("合计");
            foreach (var on in onames)
            {
                table.Columns.Add(new DataColumn(on, typeof(int)));
                var col = new telerik::GridViewDataColumn() { Header = on, UniqueName = on, DataMemberBinding = new Binding(on) };
                col.AggregateFunctions.Add(new SumFunction { ResultFormatString = "{0}件", SourceField = on, SourceFieldType = typeof(int?) });
                RadGridView1.Columns.Add(col);
            }

            var ps = data.OrderBy(o => o.ProductCode).Select(o => o.ProductID).Distinct();
            foreach (var p in ps)
            {
                var d = data.First(o => o.ProductID == p);
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                row["ProductCode"] = d.ProductCode;
                row["BrandCode"] = d.BrandCode;
                row["StyleCode"] = d.StyleCode;
                row["ColorCode"] = d.ColorCode;
                row["SizeName"] = d.SizeName;
                foreach (var on in onames)
                {
                    if (on == "合计")
                    {
                        row[on] = data.Where(o => o.ProductID == p).Sum(o => o.Quantity);
                        continue;
                    }
                    var stock = data.FirstOrDefault(o => o.ProductID == p && o.OrganizationName == on);
                    if (stock != null)
                        row[on] = stock.Quantity;
                    else
                        row[on] = 0;
                }
            }
            RadGridView1.ItemsSource = table.DefaultView;
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                SysProcessView.UIHelper.SetAPTForFilter(e.ItemPropertyDefinition.PropertyName, cbx);
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

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
