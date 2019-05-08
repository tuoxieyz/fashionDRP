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
using Telerik.Windows.Controls.GridView;
using DistributionViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillSubordinateRetailSearch.xaml
    /// </summary>
    public partial class BillSubordinateRetailSearch : UserControl
    {
        public BillSubordinateRetailSearch()
        {
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (RetailSearchEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = ReportDataContext.GetBillRetailDetails(item.ID);
            }
        }
    }
}
