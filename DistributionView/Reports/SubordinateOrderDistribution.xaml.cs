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
using DomainLogicEncap;
using DistributionViewModel;
using telerik = Telerik.Windows.Controls;
using System.Dynamic;
using System.Data;
using System.Xml.Linq;
using System.Xml;
using System.Windows.Markup;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SubordinateOrderDistribution.xaml
    /// </summary>
    public partial class SubordinateOrderDistribution : UserControl
    {
        SubordinateOrderDistributionVM _dataContext = new SubordinateOrderDistributionVM();

        public SubordinateOrderDistribution()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RadGridView1.ItemsSource = null;
            while (RadGridView1.Columns.Count > 5)
                RadGridView1.Columns.RemoveAt(5);
            var data = _dataContext.GetSubordinateOrderDistribution();
            bool showAll = rbAllOrder.IsChecked.Value;
            if (!showAll)
                data.RemoveAll(o => o.QuaDelivered == o.Quantity);
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
                table.Columns.Add(new DataColumn("delivered" + on, typeof(int)));
                table.Columns.Add(new DataColumn("all" + on, typeof(int)));
                //RadGridView1.Columns.Add(new telerik::GridViewDataColumn() { Header = on, Name = on, DataMemberBinding = new Binding(on) });
                var col = new telerik::GridViewDataColumn() { Header = on, Name = on, DataMemberBinding = new Binding(on) };
                col.AggregateFunctions.Add(new SumFunction { ResultFormatString = "{0}件", SourceField = on, SourceFieldType = typeof(int?) });
                //内存中动态生成一个XAML，描述了一个DataTemplate
                XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                XElement xGrid = new XElement(ns + "Grid", new XElement(ns + "Grid.ColumnDefinitions", new XElement(ns + "ColumnDefinition", new XAttribute("Width", "Auto")), new XElement(ns + "ColumnDefinition", new XAttribute("Width", "Auto"))));
                xGrid.Add(new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + on + "}"), new XAttribute("Margin", "0 0 5 0")));
                if (showAll)
                {
                    xGrid.Add(
                        new XElement(ns + "TextBlock", new XAttribute("Foreground", "Red"), new XAttribute("Text", "{Binding Path=" + "delivered" + on + "}"), new XAttribute("FontSize", "8"), new XAttribute("Grid.Column", "1")));
                }
                else
                {
                    xGrid.Add(
                        new XElement(ns + "TextBlock", new XAttribute("Foreground", "Red"), new XAttribute("FontSize", "8"), new XAttribute("Grid.Column", "1"),
                        new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + "all" + on + "}")),
                        new XElement(ns + "TextBlock", new XAttribute("Text", "-")),
                        new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + "delivered" + on + "}"))));
                }
                XElement xDataTemplate = new XElement(ns + "DataTemplate", new XAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
                xDataTemplate.Add(xGrid);
                XmlReader xr = xDataTemplate.CreateReader();
                DataTemplate dataTemplate = XamlReader.Load(xr) as DataTemplate;
                col.CellTemplate = dataTemplate;
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
                        var totalData = data.Where(o => o.ProductID == p);
                        var quantity = totalData.Sum(o => o.Quantity);
                        var quaDelivered = totalData.Sum(o => o.QuaDelivered);
                        row[on] = showAll ? quantity : (quantity - quaDelivered);
                        row["all" + on] = quantity;
                        row["delivered" + on] = quaDelivered;
                        continue;
                    }
                    d = data.Find(o => o.ProductID == p && o.OrganizationName == on);
                    if (d != null)
                    {
                        row[on] = showAll ? d.Quantity : (d.Quantity - d.QuaDelivered);
                        row["all" + on] = d.Quantity;
                        row["delivered" + on] = d.QuaDelivered;
                    }
                }
            }
            RadGridView1.ItemsSource = table.DefaultView;//坑爹的DefaultView，如果直接用DataTable那么CellTemplate的Binding就有问题，无法绑定，不知是微软还是Telerik搞的鬼
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "NameID":
                        cbx.ItemsSource = VMGlobal.ProNames;
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
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
