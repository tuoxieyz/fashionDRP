using SysProcessViewModel;
using System;
using System.Collections.Generic;
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
using Telerik.Windows.Controls.Data.DataFilter;

namespace DistributionView.Reports
{
    /// <summary>
    /// ParentStockStatistics.xaml 的交互逻辑
    /// </summary>
    public partial class ParentStockStatistics : UserControl
    {
        public ParentStockStatistics()
        {
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
        }

        private void radDataFilter_EditorCreated(object sender, EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    // This is a custom editor specified through the EditorTemplateSelector.
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "NameID":
                    RadComboBox cbxName = (RadComboBox)e.Editor;
                    cbxName.ItemsSource = VMGlobal.ProNames;
                    break;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
