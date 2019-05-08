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
using Manufacturing.ViewModel;
using Telerik.Windows.Controls;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace Manufacturing.Reports
{
    /// <summary>
    /// Interaction logic for SubcontractAggregation.xaml
    /// </summary>
    public partial class SubcontractAggregation : UserControl
    {
        public SubcontractAggregation()
        {
            this.DataContext = new SubcontractAggregationVM();
            InitializeComponent();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbx = e.Editor as RadComboBox;
                cbx.ItemsSource = VMGlobal.PoweredBrands;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor, e.Editor is OuterFactorySelector);
        }
    }
}
