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
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;
using DomainLogicEncap;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Microsoft.Win32;
using System.IO;
using ERPViewModelBasic;
using SysProcessViewModel;
using System.Linq.Expressions;
using System.Data;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for StockSearch.xaml
    /// </summary>
    public partial class StockStatistics : UserControl
    {
        public StockStatistics()
        {
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            GridViewExpressionColumn expColumn = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            expColumn.Expression = expression;
        }

        private void radDataFilter_EditorCreated(object sender, EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    // This is a custom editor specified through the EditorTemplateSelector.
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "Year":
                    // This is a default editor.
                    RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
                    //dateTimePickerEditor.InputMode = Telerik.Windows.Controls.InputMode.DatePicker;
                    dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                    {
                        DateTime date = (DateTime)ee.AddedItems[0];
                        dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    };
                    break;
                case "Quarter":
                    RadComboBox cbxQuarter = (RadComboBox)e.Editor;
                    cbxQuarter.ItemsSource = VMGlobal.Quarters;
                    break;
                case "NameID":
                    RadComboBox cbxName = (RadComboBox)e.Editor;
                    cbxName.ItemsSource = VMGlobal.ProNames;
                    break;
                case "SizeID":
                    RadComboBox cbxSize = (RadComboBox)e.Editor;
                    cbxSize.ItemsSource = VMGlobal.Sizes;
                    break;
                case "StorageID":
                    RadComboBox cbxStorage = (RadComboBox)e.Editor;
                    cbxStorage.ItemsSource = StorageInfoVM.Storages;
                    break;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        //private void RadGridView1_AutoGeneratingColumn(object sender, GridViewAutoGeneratingColumnEventArgs e)
        //{
        //    if (radDataFilter.ItemPropertyDefinitions.Any(o => o.PropertyName == e.Column.UniqueName))
        //    {
        //        e.Cancel = true;
        //        return;
        //    }
        //    if (e.Column.UniqueName == "数量")
        //    {
        //        e.Column.IsGroupable = false;
        //        e.Column.AggregateFunctions.Add(new SumFunction { Caption="数量合计:", ResultFormatString="{0}件",SourceField="数量" });
        //    }
        //    if (e.Column.UniqueName == "价格小计")
        //    {
        //        ((GridViewDataColumn)e.Column).DataFormatString = "{0:C2}";
        //        e.Column.IsGroupable = false;
        //        e.Column.Width = new GridViewLength(1, GridViewLengthUnitType.Star);
        //        e.Column.AggregateFunctions.Add(new SumFunction { Caption = "价格合计:", ResultFormatString = "{0:C2}", SourceField = "价格小计" });
        //    }
        //}

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
