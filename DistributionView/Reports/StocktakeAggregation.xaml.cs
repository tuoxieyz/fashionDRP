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
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for StocktakeAggregation.xaml
    /// </summary>
    public partial class StocktakeAggregation : UserControl
    {
        public StocktakeAggregation()
        {
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            ItemPropertyDefinition[] billConditions = new[] {
                new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                new ItemPropertyDefinition { DisplayName = "盘点仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "盘点品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) },
                new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                new ItemPropertyDefinition { DisplayName = "库存更新状态", PropertyName = "Status", PropertyType = typeof(bool)}};

            billFilter.ItemPropertyDefinitions.AddRange(billConditions);

            //var dateFilters = new CompositeFilterDescriptor();
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date.AddDays(-2), false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date, false));
            //billFilter.FilterDescriptors.Add(dateFilters);
            billFilter.FilterDescriptors.Add(new FilterDescriptor("StorageID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var data = ReportDataContext.AggregateStocktake(billFilter.FilterDescriptors);
            RadGridView1.ItemsSource = data;
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
                    case "StorageID":
                        cbx.ItemsSource = StorageInfoVM.Storages;
                        break;
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
