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
using View.Extension;
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;

namespace Manufacturing.Reports
{
    /// <summary>
    /// Interaction logic for BillProductExchangeSearch.xaml
    /// </summary>
    public partial class BillProductExchangeSearch : UserControl
    {
        public BillProductExchangeSearch()
        {
            this.DataContext = new BillProductExchangeSearchVM();
            InitializeComponent();
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

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (BillProductExchangeSearchEntity)e.Row.Item;
                gv.ItemsSource = item.Details;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillProductExchangeSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("生产交接单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillProductExchangeSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("生产交接单", RadGridView1, item);
        }
    }
}
