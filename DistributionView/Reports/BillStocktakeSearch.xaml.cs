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
using DomainLogicEncap;
using DistributionViewModel;
using Telerik.Windows.Controls.GridView;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillStocktakeSearch.xaml
    /// </summary>
    public partial class BillStocktakeSearch : UserControl
    {
        public BillStocktakeSearch()
        {
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "StorageID":
                        cbx.ItemsSource = StorageInfoVM.Storages;
                        break;
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (StocktakeSearchEntity)e.Row.Item;
                gv.ItemsSource = ReportDataContext.GetBillStocktakeDetails(item.BillID);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            StocktakeSearchEntity entity = (StocktakeSearchEntity)btn.DataContext;
            var result = BillLogic.DeleteBill<BillStocktake>(entity.BillID);
            if (result.IsSucceed)
                entity.IsDeleted = true;
            MessageBox.Show(result.Message);
        }

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            StocktakeSearchEntity entity = (StocktakeSearchEntity)btn.DataContext;
            var result = BillLogic.RevertBill<BillStocktake>(entity.BillID);
            if (result.IsSucceed)
                entity.IsDeleted = false;
            MessageBox.Show(result.Message);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (StocktakeSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("盘点单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (StocktakeSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("盘点单", RadGridView1, item);
        }
    }
}
