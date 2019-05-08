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
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionViewModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Linq.Expressions;
using DistributionModel;
using DomainLogicEncap;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillSelfOrderSearch.xaml
    /// </summary>
    public partial class BillSelfOrderSearch : UserControl
    {
        //private FloatPriceHelper _fpHelper;

        public BillSelfOrderSearch()
        {
            InitializeComponent();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        //private void RadGridView1_AutoGeneratingColumn(object sender, Telerik.Windows.Controls.GridViewAutoGeneratingColumnEventArgs e)
        //{
        //    var colName = e.Column.UniqueName;
        //    if (colName == "BillID" || colName == "机构名称" || colName == "OrganizationID" || colName == "订单状态" || colName == "取消量")
        //    {
        //        e.Cancel = true;
        //        return;
        //    }
        //    if (colName == "订货数量" || colName == "已发数量")
        //    {
        //        e.Column.IsGroupable = false;
        //        e.Column.AggregateFunctions.Add(new SumFunction { Caption = colName + "合计:", ResultFormatString = "{0}件", SourceField = colName });
        //    }
        //    if (colName == "备注")
        //    {
        //        e.Column.IsGroupable = false;
        //        e.Column.Width = new GridViewLength(1, GridViewLengthUnitType.Star);
        //    }
        //}

        //private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        //{
        //    if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
        //    {
        //        var gv = (RadGridView)e.DetailsElement;
        //        //Expression<Func<ProductForOrderReport, string>> expression = o => o.Quantity == o.QuaDelivered ? "已完成" : (o.QuaDelivered == 0 ? "未发货" : ((o.Quantity > o.QuaDelivered ? "部分已发货" : "数据有误")));
        //        //GridViewExpressionColumn column = gv.Columns["Status"] as GridViewExpressionColumn;
        //        //column.Expression = expression;
        //        var item = e.Row.Item;
        //        var proBill = item.GetType().GetProperty("BillID");
        //        var billID = (int)proBill.GetValue(item, null);
        //        var details = ReportDataContext.GetBillOrderDetails(billID);
        //        //if (_fpHelper == null)
        //        //    _fpHelper = new FloatPriceHelper();
        //        //foreach (var d in details)
        //        //{
        //        //    d.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, d.ProductID);
        //        //}
        //        gv.ItemsSource = details;
        //    }
        //}

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

        private void btnBillExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (OrderSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("订货单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (OrderSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("订货单", RadGridView1, item);
        }
    }
}
