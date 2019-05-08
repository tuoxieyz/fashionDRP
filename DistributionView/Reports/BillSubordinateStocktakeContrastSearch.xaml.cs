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
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillSubordinateStocktakeContrastSearch.xaml
    /// </summary>
    public partial class BillSubordinateStocktakeContrastSearch : UserControl
    {
        public BillSubordinateStocktakeContrastSearch()
        {
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (ContrastSearchEntity)e.Row.Item;
                var details = ReportDataContext.GetBillStocktakeContrastDetails(item.ID);
                gv.ItemsSource = details;
            }
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
