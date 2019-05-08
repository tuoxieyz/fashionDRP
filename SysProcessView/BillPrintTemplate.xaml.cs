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
using Telerik.Windows.Controls.GridView;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for BillPrintTemplate.xaml
    /// </summary>
    public partial class BillPrintTemplate : UserControl
    {
        public BillPrintTemplate(string title, RadGridView gv, object item)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(title))
                title = "单据明细";
            tbTitle.Text = title;
            //View.Extension.UIHelper.DoEvents();//强制刷新UI

            List<BillPrintHeaderItem> headerItems = new List<BillPrintHeaderItem>();
            foreach (var col in gv.Columns)
            {
                if (col.Header != null)//过滤没有列头（如标识列）的列
                {
                    var colname = col.Header.ToString();
                    if (colname != "操作" && col is IExportableColumn && col.IsVisible)
                    {
                        var text = ((IExportableColumn)col).GetCellContent(item);
                        if (text == null)
                            text = "";
                        headerItems.Add(new BillPrintHeaderItem { LabelString = colname, TextString = text.ToString() });
                    }
                }
            }
            icHeader.ItemsSource = headerItems;

            var row = (GridViewRow)gv.ItemContainerGenerator.ContainerFromItem(item);//View.Extension.UIHelper.GetAncestor<GridViewRow>(sender as RadButton);
            row.DetailsVisibility = Visibility.Visible;
            var detailsPresenter = row.ChildrenOfType<DetailsPresenter>().FirstOrDefault();
            // same as e.DetailsElement from gridView_RowDetailsVisibilityChanged 
            var gvDetails = (RadGridView)detailsPresenter.Content;

            foreach (var column in gvDetails.Columns.OfType<Telerik.Windows.Controls.GridViewColumn>())
            {
                if (column.IsVisible)
                {
                    if (column is GridViewDataColumn)
                    {
                        var newColumn = new GridViewDataColumn() { Header = column.Header };
                        newColumn.DataMemberBinding = new System.Windows.Data.Binding(column.UniqueName);
                        gvData.Columns.Add(newColumn);
                    }
                    else if (column is GridViewExpressionColumn)
                    {
                        var newColumn = new GridViewExpressionColumn() { Header = column.Header };
                        newColumn.Expression = ((GridViewExpressionColumn)column).Expression;
                        gvData.Columns.Add(newColumn);
                    }
                }
            }
            gvData.ItemsSource = gvDetails.ItemsSource;
        }

        private class BillPrintHeaderItem
        {
            public string LabelString { get; set; }
            public string TextString { get; set; }
        }
    }
}
