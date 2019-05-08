using DistributionViewModel;
using SysProcessViewModel;
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

namespace DistributionView.Reports
{
    /// <summary>
    /// SubordinateStoingSaleStockContrail.xaml 的交互逻辑
    /// </summary>
    public partial class SubordinateStoingSaleStockContrail : UserControl
    {
        SubordinateStoingSaleStockContrailVM _dataContext = new SubordinateStoingSaleStockContrailVM();

        public SubordinateStoingSaleStockContrail()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "NameID":
                    RadComboBox cbxName = (RadComboBox)e.Editor;
                    cbxName.ItemsSource = VMGlobal.ProNames;
                    break;
                case "Year":
                    RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
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
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
