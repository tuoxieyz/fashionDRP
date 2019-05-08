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
using View.Extension;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for OrganizationFundAccountTotal.xaml
    /// </summary>
    public partial class OrganizationFundAccountTotal : UserControl
    {
        OrganizationFundAccountTotalVM _dataContext = new OrganizationFundAccountTotalVM();

        public OrganizationFundAccountTotal()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (FundAccountTotalEntity)e.Row.Item;
                var grid = (Grid)e.DetailsElement;
                var pager = grid.FindChildByType<RadDataPager>();
                this.SearchDetails(pager, item, grid);
            }
        }

        private void RadDataPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
        {
            RadDataPager pager = sender as RadDataPager;
            this.SearchDetails(pager, (FundAccountTotalEntity)pager.DataContext);
        }

        private void SearchDetails(RadDataPager pager, FundAccountTotalEntity totalEntity, Grid grid = null)
        {
            if (totalEntity != null)
            {
                int totalCount = 0;
                grid = grid ?? UIHelper.GetAncestor<Grid>(pager);
                var gv = grid.FindChildByType<RadGridView>();
                gv.ItemsSource = _dataContext.SearchFundAccount(totalEntity.OrganizationID, totalEntity.BrandID, pager.PageIndex, pager.PageSize, ref totalCount);
                pager.ItemCount = totalCount;
            }
        }
    }
}
