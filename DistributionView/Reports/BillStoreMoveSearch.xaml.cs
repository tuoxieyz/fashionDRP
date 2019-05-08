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
using DistributionModel;
using DomainLogicEncap;
using DistributionViewModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillStoreMoveSearch.xaml
    /// </summary>
    public partial class BillStoreMoveSearch : UserControl
    {
        private FloatPriceHelper _fpHelper;

        public BillStoreMoveSearch()
        {
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "StorageIDOut" || e.ItemPropertyDefinition.PropertyName == "StorageIDIn")
            {
                RadComboBox cbxStorage = (RadComboBox)e.Editor;
                cbxStorage.ItemsSource = StorageInfoVM.Storages;
            }
            else if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbxBrand = (RadComboBox)e.Editor;
                cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (StoreMoveSearchEntity)e.Row.Item;
                var details = ReportDataContext.SearchBillDetails<BillStoreMoveDetails>(item.ID);
                if (_fpHelper == null)
                    _fpHelper = new FloatPriceHelper();
                foreach (var d in details)
                {
                    d.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, d.BYQID, d.Price);
                }
                gv.ItemsSource = details;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void btnBillExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (StoreMoveSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("出库单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (StoreMoveSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("出库单", RadGridView1, item);
        }
    }
}
