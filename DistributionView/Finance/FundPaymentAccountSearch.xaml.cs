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
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for FundPaymentAccountSearch.xaml
    /// </summary>
    public partial class FundPaymentAccountSearch : UserControl
    {
        OrganizationFundAccountSearchVM _dataContext = new OrganizationFundAccountSearchVM();

        public FundPaymentAccountSearch()
        {
            this.DataContext = _dataContext;
            _dataContext.OrganizationArray = new SysOrganization[] { OrganizationListVM.CurrentOrganization };
            InitializeComponent();
        }

        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    int totalCount = 0;
        //    CompositeFilterDescriptorCollection filters = new CompositeFilterDescriptorCollection();
        //    filters.Add(radDataFilter.FilterDescriptors);
        //    filters.Add(new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, VMGlobal.CurrentUser.OrganizationID));
        //    RadGridView1.ItemsSource = ReportDataContext.SearchFundAccount(filters, pager.PageIndex, pager.PageSize, ref totalCount);
        //    pager.ItemCount = totalCount;
        //}

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
