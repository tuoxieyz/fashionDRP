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
using Telerik.Windows.Controls;
using ERPViewModelBasic;
using DistributionViewModel;
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for AvailableStockStatistics.xaml
    /// </summary>
    public partial class AvailableStockStatistics : UserControl
    {
        public AvailableStockStatistics()
        {
            InitializeComponent();
            //RadGridView1.RowDetailsTemplate = null;
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    // This is a custom editor specified through the EditorTemplateSelector.
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "Year":
                    // This is a default editor.
                    RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
                    //dateTimePickerEditor.InputMode = Telerik.Windows.Controls.InputMode.DatePicker;
                    dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                    {
                        DateTime date = (DateTime)ee.AddedItems[0];
                        dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    };
                    break;
                case "Quarter":
                    RadComboBox cbxQuarter = (RadComboBox)e.Editor;
                    cbxQuarter.ItemsSource = VMGlobal.Quarters;
                    break;
                case "NameID":
                    RadComboBox cbxName = (RadComboBox)e.Editor;
                    cbxName.ItemsSource = VMGlobal.ProNames;
                    break;
                case "SizeID":
                    RadComboBox cbxSize = (RadComboBox)e.Editor;
                    cbxSize.ItemsSource = VMGlobal.Sizes;
                    break;
                case "StorageID":
                    RadComboBox cbxStorage = (RadComboBox)e.Editor;
                    cbxStorage.ItemsSource = StorageInfoVM.Storages;
                    break;
            }

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (AvailableStockStatisticsEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = item.Details;
            }
        }
    }
}
