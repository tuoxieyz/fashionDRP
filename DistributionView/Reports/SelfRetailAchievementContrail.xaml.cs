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
using DistributionViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SelfRetailAchievementContrail.xaml
    /// </summary>
    public partial class SelfRetailAchievementContrail : UserControl
    {
        public SelfRetailAchievementContrail()
        {
            InitializeComponent();

            billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime) });
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var data = ReportDataContext.GetSelfRetailAchievement(billFilter.FilterDescriptors);
            RadGridView1.ItemsSource = data;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
