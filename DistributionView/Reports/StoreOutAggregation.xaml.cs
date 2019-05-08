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

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for StoreOutAggregation.xaml
    /// </summary>
    public partial class StoreOutAggregation : UserControl
    {
        public StoreOutAggregation()
        {
            this.DataContext = new StoreOutAggregationVM();
            InitializeComponent();

            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
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
