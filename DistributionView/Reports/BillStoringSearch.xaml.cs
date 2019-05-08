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
using DomainLogicEncap;
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.GridView;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillStoringSearch.xaml
    /// </summary>
    public partial class BillStoringSearch : UserControl
    {
        private FloatPriceHelper _fpHelper;

        ObservableCollection<FilterDescriptor> _billFilterDescriptors;

        public ObservableCollection<FilterDescriptor> BillFilterDescriptors
        {
            get
            {
                if (_billFilterDescriptors == null)
                {
                    _billFilterDescriptors = new ObservableCollection<FilterDescriptor>() 
                    {  
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BillType", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false)
                    };
                }

                return _billFilterDescriptors;
            }
        }

        public BillStoringSearch()
        {
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "StorageID")
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

        private void RadGridView1_AutoGeneratingColumn(object sender, GridViewAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.UniqueName == "BillID" || e.Column.UniqueName == "StorageID" || e.Column.UniqueName == "CreateTime" || e.Column.UniqueName == "BillType" || e.Column.UniqueName == "BrandID")
            {
                e.Cancel = true;
                return;
            }
            else if (e.Column.UniqueName == "入库数量")
            {
                e.Column.IsGroupable = false;
                e.Column.AggregateFunctions.Add(new SumFunction { Caption = "数量合计:", ResultFormatString = "{0}件", SourceField = "入库数量" });
            }
            else if (e.Column.UniqueName == "备注")
            {
                e.Column.IsGroupable = false;
                e.Column.Width = new GridViewLength(1, GridViewLengthUnitType.Star);
            }
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = e.Row.Item;
                var proBill = item.GetType().GetProperty("BillID");
                var billID = (int)proBill.GetValue(item, null);
                var details = ReportDataContext.SearchBillDetails<BillStoringDetails>(billID);
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
            var item = (StoringSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("入库单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (StoringSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("入库单", RadGridView1, item);
        }
    }
}
