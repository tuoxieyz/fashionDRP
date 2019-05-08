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
using System.Data;
using telerik = Telerik.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Markup;
using System.Xml;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SubordinateCannibalizeDistribution.xaml
    /// </summary>
    public partial class SubordinateCannibalizeDistribution : UserControl
    {
        public SubordinateCannibalizeDistribution()
        {
            InitializeComponent();

            ItemPropertyDefinition[] billConditions = new[] {
                new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                new ItemPropertyDefinition { DisplayName = "调拨品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)}};
            billFilter.ItemPropertyDefinitions.AddRange(billConditions);
            billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "调出机构", PropertyName = "OrganizationID", PropertyType = typeof(int) });
            billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "调入机构", PropertyName = "ToOrganizationID", PropertyType = typeof(int) });
            billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(bool) });

            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("ToOrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RadGridView1.ItemsSource = null;
            while (RadGridView1.Columns.Count > 5)
                RadGridView1.Columns.RemoveAt(5);
            var data = ReportDataContext.GetSubordinateCannibalizeDistribution(billFilter.FilterDescriptors);
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("ProductCode", typeof(string)));
            table.Columns.Add(new DataColumn("BrandCode", typeof(string)));
            table.Columns.Add(new DataColumn("StyleCode", typeof(string)));
            table.Columns.Add(new DataColumn("ColorCode", typeof(string)));
            table.Columns.Add(new DataColumn("SizeName", typeof(string)));
            var onames = data.Select(o => o.OutOrganizationName).Concat(data.Select(o => o.InOrganizationName)).Distinct().ToList();
            foreach (var on in onames)
            {
                table.Columns.Add(new DataColumn(on, typeof(int)));
                table.Columns.Add(new DataColumn("cannibalizein" + on, typeof(int)));
                var col = new telerik::GridViewDataColumn() { Header = on, Name = on, DataMemberBinding = new Binding(on) };
                col.AggregateFunctions.Add(new CannibalizeTotalFunction(on) { ResultFormatString = "{0}" });
                //内存中动态生成一个XAML，描述了一个DataTemplate
                XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                XElement xGrid = new XElement(ns + "Grid");
                xGrid.Add(
                    new XElement(ns + "TextBlock",
                    new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + on + "}")),
                    new XElement(ns + "TextBlock", new XAttribute("Text", " - "), new XAttribute("Foreground", "Red")),
                    new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + "cannibalizein" + on + "}"))));
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
                    var ds = data.FindAll(o => o.ProductID == p && o.OutOrganizationName == on);
                    if (ds != null && ds.Count > 0)
                        row[on] = ds.Sum(o => o.Quantity);
                    else
                        row[on] = 0;
                    ds = data.FindAll(o => o.ProductID == p && o.InOrganizationName == on);
                    if (ds != null && ds.Count > 0)
                        row["cannibalizein" + on] = ds.Sum(o => o.Quantity);
                    else
                        row["cannibalizein" + on] = 0;
                }
            }
            RadGridView1.ItemsSource = table.DefaultView;
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
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private class CannibalizeTotalFunction : AggregateFunction<DataRowView, string>
        {
            public CannibalizeTotalFunction(string colname)
            {
                this.AggregationExpression = rows => rows.Sum(r => (int)r[colname]).ToString() + "-" + rows.Sum(r => (int)r["cannibalizein" + colname]).ToString();
            }
        }
    }
}
