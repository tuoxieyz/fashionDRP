using DistributionViewModel;
using Microsoft.Win32;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
using Telerik.Windows.Controls.ChartView;
using Telerik.Windows.Controls.Data.DataFilter;

namespace DistributionView.Reports
{
    /// <summary>
    /// StockABCAnalysis.xaml 的交互逻辑
    /// </summary>
    public partial class StockABCAnalysis : UserControl
    {
        StockABCAnalysisVM _dataContext = new StockABCAnalysisVM();

        public StockABCAnalysis()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
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
                case "StorageID":
                    RadComboBox cbxStorage = (RadComboBox)e.Editor;
                    cbxStorage.ItemsSource = StorageInfoVM.Storages;
                    break;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel Worksheets (*.xlsx)|*.xlsx";
            if (!(bool)dialog.ShowDialog())
            {
                return;
            }
            using (Stream fileStream = dialog.OpenFile())
            {
                Telerik.Windows.Media.Imaging.ExportExtensions.ExportToExcelMLImage(this.chart, fileStream);
            }
        }

        private void radListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (radListBox.SelectedItem != null && _dataContext.Entities != null)
            {
                List<ABCEntity> entities = null;
                string content = (radListBox.SelectedItem as RadListBoxItem).Content.ToString();
                switch (content)
                {
                    case "款式":
                        lineSeries.CategoryBinding = barSeries.CategoryBinding = new PropertyNameDataPointBinding("StyleCode");
                        entities = _dataContext.Entities.GroupBy(o => o.StyleCode).Select(g => new ABCEntity
                        {
                            StyleCode = g.Key,
                            CostMoney = g.Sum(o => o.Price * o.Quantity),
                            Quantity = g.Sum(o => o.Quantity)
                        }).ToList();
                        break;
                    case "颜色":
                        lineSeries.CategoryBinding = barSeries.CategoryBinding = new PropertyNameDataPointBinding("ColorName");
                        entities = _dataContext.Entities.GroupBy(o => o.ColorCode).Select(g => new ABCEntity
                        {
                            ColorName = VMGlobal.Colors.Find(o => o.Code == g.Key).Name,
                            CostMoney = g.Sum(o => o.Price * o.Quantity),
                            Quantity = g.Sum(o => o.Quantity)
                        }).ToList();
                        break;
                    default:
                        lineSeries.CategoryBinding = barSeries.CategoryBinding = new PropertyNameDataPointBinding("Name");
                        entities = _dataContext.Entities.GroupBy(o => o.ProductName).Select(g => new ABCEntity
                        {
                            Name = g.Key,
                            CostMoney = g.Sum(o => o.Price * o.Quantity),
                            Quantity = g.Sum(o => o.Quantity)
                        }).ToList();
                        break;
                }
                decimal accumulativeProportion = 0;
                bool ismoney = btnMoney.IsChecked.Value;//左侧纵坐标表示金额or数量
                if (ismoney)
                {
                    barSeries.ValueBinding = new PropertyNameDataPointBinding("CostMoney");
                    entities = entities.OrderByDescending(o => o.CostMoney).ToList();
                    foreach (var abc in entities)
                    {
                        abc.Proportion = abc.CostMoney / _dataContext.AmountCostMoney;
                        accumulativeProportion += abc.Proportion;
                        abc.AccuProportion = accumulativeProportion;
                    }
                }
                else
                {
                    barSeries.ValueBinding = new PropertyNameDataPointBinding("Quantity");
                    entities = entities.OrderByDescending(o => o.Quantity).ToList();
                    foreach (var abc in entities)
                    {
                        abc.Proportion = abc.Quantity * 1.0M / _dataContext.AmountQuantity;
                        accumulativeProportion += abc.Proportion;
                        abc.AccuProportion = accumulativeProportion;
                    }
                }
                lineSeries.ItemsSource = barSeries.ItemsSource = entities;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.SearchCommand.Execute(null);
            radListBox_SelectionChanged(null, null);
        }

        private void btnMoney_Checked(object sender, RoutedEventArgs e)
        {
            if (radListBox != null)
                radListBox_SelectionChanged(null, null);
            if (leftAxis != null)
            {
                leftAxis.Title = btnMoney.IsChecked.Value ? "金额(元)" : "数量(件)";
            }
        }
    }
}
