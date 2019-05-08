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
using DistributionViewModel;
using Telerik.Windows.Controls;
using SysProcessViewModel;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for OrganizationFundAccountSearch.xaml
    /// </summary>
    public partial class OrganizationFundAccountSearch : UserControl
    {
        OrganizationFundAccountSearchVM _dataContext = new OrganizationFundAccountSearchVM();

        public OrganizationFundAccountSearch()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                if (e.ItemPropertyDefinition.PropertyName == "BrandID")
                {
                    cbx.ItemsSource = VMGlobal.PoweredBrands;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
