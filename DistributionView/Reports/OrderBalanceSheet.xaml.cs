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
using SysProcessModel;
using Telerik.Windows.Controls;
using SysProcessView;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for OrderBalanceSheet.xaml
    /// </summary>
    public partial class OrderBalanceSheet : UserControl
    {
        OrderBalanceSheetVM _dataContext = new OrderBalanceSheetVM();

        public OrderBalanceSheet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.SearchData();
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

        private void RadGridView1_AutoGeneratingColumn(object sender, GridViewAutoGeneratingColumnEventArgs e)
        {
            string colname = e.Column.Header.ToString();
            switch (colname)
            {
                case "品牌":
                case "款号":
                case "色号":
                case "尺码":
                    break;
                default:
                    e.Column.IsGroupable = false;
                    break;
            }
        }
    }
}
