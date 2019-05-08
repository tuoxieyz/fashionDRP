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
using Telerik.Windows.Controls.GridView;
using Manufacturing.ViewModel;
using View.Extension;
using SysProcessViewModel;

namespace Manufacturing.Reports
{
    /// <summary>
    /// Interaction logic for BillSubcontractSearch.xaml
    /// </summary>
    public partial class BillSubcontractSearch : UserControl
    {
        public BillSubcontractSearch()
        {
            this.DataContext = new BillSubcontractSearchVM();
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
                var item = (BillSubcontractSearchEntity)e.Row.Item;
                gv.ItemsSource = item.Details;
                foreach (var d in item.Details)
                {
                    if (d.DeliveryDate < DateTime.Now.Date && d.Status != "已完成")//过期未完成
                    {
                        var row = gv.ItemContainerGenerator.ContainerFromItem(d) as GridViewRow;
                        UIHelper.SetGridRowValidBackground(row, false);
                    }
                }
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillSubcontractSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("外发生产单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillSubcontractSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("外发生产单", RadGridView1, item);
        }
    }
}
