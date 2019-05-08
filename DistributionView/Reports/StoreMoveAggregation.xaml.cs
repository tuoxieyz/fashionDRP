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
using System.Xml.Linq;
using telerik = Telerik.Windows.Controls;
using System.Xml;
using System.Windows.Markup;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for StoreMoveAggregation.xaml
    /// </summary>
    public partial class StoreMoveAggregation : UserControl
    {
        public StoreMoveAggregation()
        {
            InitializeComponent();

            ItemPropertyDefinition[] billConditions = new[] {
                new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},                
                new ItemPropertyDefinition { DisplayName = "移出仓库", PropertyName = "StorageIDOut", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "移入仓库", PropertyName = "StorageIDIn", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "移库品牌", PropertyName = "BrandID", PropertyType = typeof(int)}};

            billFilter.ItemPropertyDefinitions.AddRange(billConditions);

            //var dateFilters = new CompositeFilterDescriptor();
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            //billFilter.FilterDescriptors.Add(dateFilters);

            billFilter.FilterDescriptors.AddRange(new List<FilterDescriptor>
            {  
                new FilterDescriptor("StorageIDOut", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                new FilterDescriptor("StorageIDIn", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false)
            });
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "StorageIDOut" || e.ItemPropertyDefinition.PropertyName == "StorageIDIn")
            {
                RadComboBox cbxStorage = (RadComboBox)e.Editor;
                cbxStorage.ItemsSource = StorageInfoVM.Storages;
            }
            else if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbxBrand = (RadComboBox)e.Editor;
                cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RadGridView1.ItemsSource = null;
            while (RadGridView1.Columns.Count > 5)
                RadGridView1.Columns.RemoveAt(5);
            var data = ReportDataContext.AggregateStoreMove(billFilter.FilterDescriptors);
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("ProductCode", typeof(string)));
            table.Columns.Add(new DataColumn("BrandCode", typeof(string)));
            table.Columns.Add(new DataColumn("StyleCode", typeof(string)));
            table.Columns.Add(new DataColumn("ColorCode", typeof(string)));
            table.Columns.Add(new DataColumn("SizeName", typeof(string)));
            var snames = ReportDataContext.Storages.Select(o => o.Name).ToList();
            foreach (var sn in snames)
            {
                table.Columns.Add(new DataColumn(sn, typeof(int)));
                table.Columns.Add(new DataColumn("movein" + sn, typeof(int)));
                var col = new telerik::GridViewDataColumn() { Header = sn, UniqueName = sn, DataMemberBinding = new Binding(sn) };
                col.AggregateFunctions.Add(new StoreMoveTotalFunction(sn) { Caption = "出入合计:", ResultFormatString = "{0}" });
                //内存中动态生成一个XAML，描述了一个DataTemplate
                XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                XElement xGrid = new XElement(ns + "Grid");
                xGrid.Add(
                    new XElement(ns + "TextBlock",
                    new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + sn + "}")),
                    new XElement(ns + "TextBlock", new XAttribute("Text", " - "), new XAttribute("Foreground", "Red")),
                    new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + "movein" + sn + "}"))));
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
                foreach (var sn in snames)
                {
                    var ds = data.FindAll(o => o.ProductID == p && o.OutStorageName == sn);
                    if (ds != null && ds.Count > 0)
                        row[sn] = ds.Sum(o => o.Quantity);
                    else
                        row[sn] = 0;
                    ds = data.FindAll(o => o.ProductID == p && o.InStorageName == sn);
                    if (ds != null && ds.Count > 0)
                        row["movein" + sn] = ds.Sum(o => o.Quantity);
                    else
                        row["movein" + sn] = 0;
                }
            }
            RadGridView1.ItemsSource = table.DefaultView;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private class StoreMoveTotalFunction : AggregateFunction<DataRowView, string>
        {
            public StoreMoveTotalFunction(string colname)
            {
                this.AggregationExpression = rows => rows.Sum(r => (int)r[colname]).ToString() + "-" + rows.Sum(r => (int)r["movein" + colname]).ToString();
            }
        }
    }
}
