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
using SysProcessViewModel;
using System.Linq.Expressions;
using System.Data;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SelfGoodReturnAggregation.xaml
    /// </summary>
    public partial class SelfGoodReturnAggregation : UserControl
    {
        public SelfGoodReturnAggregation()
        {
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            GridViewExpressionColumn expColumn = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            expColumn.Expression = expression;

            ItemPropertyDefinition[] billConditions = new[] {
                new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                new ItemPropertyDefinition { DisplayName = "退货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) }};
            billFilter.ItemPropertyDefinitions.AddRange(billConditions);

            var dateFilters = new CompositeFilterDescriptor();
            dateFilters.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            dateFilters.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(dateFilters);
            billFilter.FilterDescriptors.Add(new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false));

            billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) });
            billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(bool) });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var data = ReportDataContext.AggregateSelfGoodReturn(billFilter.FilterDescriptors);
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
