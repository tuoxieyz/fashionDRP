using DistributionViewModel;
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

namespace DistributionView.Reports
{
    /// <summary>
    /// RetailDayReport.xaml 的交互逻辑
    /// </summary>
    public partial class RetailDayReport : UserControl
    {
        RetailDayReportVM _dataContext = new RetailDayReportVM();

        public RetailDayReport()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            btnSearch.Click += btnSearch_Click;
        }

        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.SearchData();
        }
    }
}
