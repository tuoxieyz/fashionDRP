using DistributionViewModel;
using Microsoft.Win32;
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

namespace DistributionView.Reports
{
    /// <summary>
    /// RetailABCAnalysis.xaml 的交互逻辑
    /// </summary>
    public partial class RetailABCAnalysis : UserControl
    {
        RetailABCAnalysisVM _dataContext = new RetailABCAnalysisVM();

        public RetailABCAnalysis()
        {
            this.DataContext = _dataContext;
            this.Resources["Context"] = _dataContext;
            InitializeComponent();
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

        //click事件在command之前
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.SearchData();
            radListBox_SelectionChanged(null, null);
        }

        private void radListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (radListBox.SelectedItem != null)
            {
                string content = (radListBox.SelectedItem as RadListBoxItem).Content.ToString();
                switch (content)
                {
                    case "款式":
                        lineSeries.CategoryBinding = barSeries.CategoryBinding = new PropertyNameDataPointBinding("StyleCode");
                        lineSeries.ItemsSource = barSeries.ItemsSource = _dataContext.StyleABCEntity;
                        break;
                    case "颜色":
                        lineSeries.CategoryBinding = barSeries.CategoryBinding = new PropertyNameDataPointBinding("ColorName");
                        lineSeries.ItemsSource = barSeries.ItemsSource = _dataContext.ColorABCEntity;
                        break;
                    default:
                        lineSeries.CategoryBinding = barSeries.CategoryBinding = new PropertyNameDataPointBinding("Name");
                        lineSeries.ItemsSource = barSeries.ItemsSource = _dataContext.ProNameABCEntity;
                        break;
                }
            }
        }
    }
}
