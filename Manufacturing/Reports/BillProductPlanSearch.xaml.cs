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
using Manufacturing.ViewModel;
using SysProcessViewModel;
using Telerik.Windows.Controls.GridView;

namespace Manufacturing.Reports
{
    /// <summary>
    /// Interaction logic for BillProductPlanSearch.xaml
    /// </summary>
    public partial class BillProductPlanSearch : UserControl
    {
        public BillProductPlanSearch()
        {
            this.DataContext = new BillProductPlanSearchVM();
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbx = e.Editor as RadComboBox;
                cbx.ItemsSource = VMGlobal.PoweredBrands;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (BillProductPlanSearchEntity)e.Row.Item;
                gv.ItemsSource = item.Details;
                foreach (var d in item.Details)
                {
                    if (d.DeliveryDate < DateTime.Now.Date && d.Status != "已完成")//过期未完成
                    {
                        var row = gv.ItemContainerGenerator.ContainerFromItem(d) as GridViewRow;
                        View.Extension.UIHelper.SetGridRowValidBackground(row, false);
                    }
                }
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillProductPlanSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("生产计划单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillProductPlanSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("生产计划单", RadGridView1, item);
        }
    }
}
