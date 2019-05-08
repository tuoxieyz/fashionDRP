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

using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using SysProcessViewModel;
using System.Data;
using System.Linq.Expressions;

namespace DistributionView.Reports
{
    /// <summary>
    /// SelfOrderAggregation.xaml 的交互逻辑
    /// </summary>
    public partial class SelfOrderAggregation : UserControl
    {
        SelfOrderAggregationVM _dataContext = new SelfOrderAggregationVM();

        public SelfOrderAggregation()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            //SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            //Expression<Func<DataRow, decimal>> expression = prod => (decimal)prod["Price"] * (int)prod["Quantity"];
            //GridViewExpressionColumn colPriceSubTotal = RadGridView1.Columns["colPriceSubTotal"] as GridViewExpressionColumn;
            //colPriceSubTotal.Expression = expression;
            //Expression<Func<DataRow, decimal>> expression2 = prod => (int)prod["Quantity"] - (int)prod["QuaDelivered"];
            //GridViewExpressionColumn colNoDelivered = RadGridView1.Columns["colNoDelivered"] as GridViewExpressionColumn;
            //colNoDelivered.Expression = expression2;
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

        //private void ckShowZero_Checked(object sender, RoutedEventArgs e)
        //{
        //    var data = RadGridView1.ItemsSource as ObservableCollection<OrderAggregationEntity>;
        //    if (data != null)
        //        btnSearch_Click(null, null);
        //}

        //private void ckShowZero_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    var data = RadGridView1.ItemsSource as ObservableCollection<OrderAggregationEntity>;
        //    if (data != null)
        //    {
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            var d = data[i];
        //            if (d.QuaDelivered == d.Quantity)
        //            {
        //                data.Remove(d);
        //                i--;
        //            }
        //        }
        //    }
        //}

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
