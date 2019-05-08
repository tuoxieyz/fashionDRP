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
using DomainLogicEncap;
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls;
using DistributionModel;
using View.Extension;
using SysProcessViewModel;
using SysProcessView;
using ERPViewModelBasic;
using System.Linq.Expressions;
using System.Data;

namespace DistributionView.Reports
{
    /// <summary>
    /// BillDeliverySearch.xaml 的交互逻辑
    /// </summary>
    public partial class BillDeliverySearch : UserControl
    {
        Expression<Func<DataRow, decimal>> _expression = prod => (decimal)prod["Price"] * (decimal)prod["Discount"] / 100;

        public BillDeliverySearch()
        {
            //var childOrgs = OrganizationLogic.GetChildOrganizations(VMGlobal.CurrentUser.OrganizationID);
            //if (childOrgs == null || childOrgs.Count == 0)
            //{
            //    MessageBox.Show("没有下级机构，该菜单不可用");
            //    return;
            //}
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (DeliverySearchEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                if (gv.Tag == null)
                {
                    gv.Tag = new object();
                    SysProcessView.UIHelper.TransferSizeToHorizontal(gv);

                    GridViewExpressionColumn expColumn = gv.Columns["colDiscountPrice"] as GridViewExpressionColumn;
                    expColumn.Expression = _expression;
                }
                //var sizeCol = gv.Columns.FirstOrDefault<Telerik.Windows.Controls.GridViewColumn>(o => o.Header.ToString() == "尺码");
                //if (sizeCol != null)
                //{
                //    int index = gv.Columns.IndexOf(sizeCol);
                //    gv.Columns.RemoveAt(index);
                //    foreach (var size in VMGlobal.Sizes)
                //    {
                //        var col = new GridViewDataColumn() { Header = size.Name, UniqueName = size.Name, DataMemberBinding = new Binding(size.Name) };
                //        gv.Columns.Insert(index, col);
                //        index++;
                //    }
                //}
                gv.ItemsSource = new BillReportHelper().TransferSizeToHorizontal<DistributionProductShow>(item.Details, propertyNamesForSum: new string[] { "Quantity", "SettlementPrice" });
                //gv.ItemsSource = item.Details;
            }
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

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (DeliverySearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("发货单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (DeliverySearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("发货单", RadGridView1, item);
        }
    }
}
