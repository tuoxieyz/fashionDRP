using DistributionViewModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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

namespace DistributionView.Reports
{
    /// <summary>
    /// SelfOrderAggregationNew.xaml 的交互逻辑
    /// </summary>
    public partial class SelfOrderAggregationNew : UserControl
    {
        SelfOrderAggregationNewVM _dataContext = new SelfOrderAggregationNewVM();

        public SelfOrderAggregationNew()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            GridViewExpressionColumn colPriceSubTotal = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            colPriceSubTotal.Expression = expression;
            _dataContext.PropertyChanged += _dataContext_PropertyChanged;
        }

        void _dataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsShowStock" && _dataContext.IsShowStock)
                RadGridView1.CalculateAggregates();
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
                    case "StorageID":
                        cbx.ItemsSource = StorageInfoVM.Storages;
                        break;
                    case "NameID":
                        cbx.ItemsSource = VMGlobal.ProNames;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
