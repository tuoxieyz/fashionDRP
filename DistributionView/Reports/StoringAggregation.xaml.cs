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
using Telerik.Windows.Controls;
using DomainLogicEncap;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataFilter;
using SysProcessViewModel;
using System.Linq.Expressions;
using System.Data;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillStoringAggregation.xaml
    /// </summary>
    public partial class StoringAggregation : UserControl
    {
        public StoringAggregation()
        {
            this.DataContext = new StoringAggregationVM();
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            GridViewExpressionColumn expColumn = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            expColumn.Expression = expression;

            //var dateFilters = new CompositeFilterDescriptor();
            //dateFilters.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            //dateFilters.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false));
            //billFilter.FilterDescriptors.Add(dateFilters);            
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "StorageID")
            {
                RadComboBox cbxStorage = (RadComboBox)e.Editor;
                cbxStorage.ItemsSource = StorageInfoVM.Storages;
            }
            else if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbxBrand = (RadComboBox)e.Editor;
                cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            }
            else if (e.ItemPropertyDefinition.PropertyName == "NameID")
            {
                RadComboBox cbxBrand = (RadComboBox)e.Editor;
                cbxBrand.ItemsSource = VMGlobal.ProNames;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
