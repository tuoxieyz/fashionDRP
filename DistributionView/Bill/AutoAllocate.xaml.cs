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
using DistributionViewModel;
using DistributionModel;
using SysProcessModel;
using Telerik.Windows.Data;
using System.Xml.Linq;
using System.Windows.Markup;
using System.Xml;
using System.Data;
using SysProcessView;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for AutoAllocate.xaml
    /// </summary>
    public partial class AutoAllocate : UserControl
    {
        AutoAllocateVM _dataContext = new AutoAllocateVM();

        public AutoAllocate()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RadGridView1.Columns.Clear();
            var result = _dataContext.SearchData();
            if (!result.IsSucceed)
                MessageBox.Show(result.Message);
        }

        private void btnSelectStyles_Click(object sender, RoutedEventArgs e)
        {
            StyleSelectWin win = (_dataContext.BrandID == default(int) ? new StyleSelectWin() : new StyleSelectWin(_dataContext.BrandID, brandSeletable: true, styleIDsSeleted: _dataContext.Styles.Select(o => o.ID)));
            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.SetCompleted += delegate(IEnumerable<ProStyle> styles)
            {
                _dataContext.Styles = styles;
                if (styles == null || styles.Count() == 0)
                {
                    tbStyles.Clear();
                }
                else
                {
                    string info = "";
                    foreach (var o in _dataContext.Styles)
                    {
                        info += o.Code + ",";
                    }
                    tbStyles.Text = info.TrimEnd(',');
                }
                win.Close();
            };
            win.ShowDialog();
        }

        //private void btnSelectOrganizations_Click(object sender, RoutedEventArgs e)
        //{
        //    MultiOrganizationSelectWin win = (_dataContext.Organizations == null ? new MultiOrganizationSelectWin() : new MultiOrganizationSelectWin(selectedIDs: _dataContext.Organizations.Select(o => o.ID)));
        //    win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
        //    win.SetCompleted += delegate(IEnumerable<SysOrganization> organizations)
        //    {
        //        _dataContext.Organizations = organizations;
        //    };
        //    win.ShowDialog();
        //}

        //private void RadGridView1_AutoGeneratingColumn(object sender, Telerik.Windows.Controls.GridViewAutoGeneratingColumnEventArgs e)
        //{
        //    int index = RadGridView1.Columns.IndexOf(e.Column);//此时一直为-1
        //    if (index < 6 || index == RadGridView1.Columns.Count - 1)
        //        e.Column.IsReadOnly = true;
        //    else
        //    {
        //        e.Column.IsReadOnly = false;
        //    }
        //}

        private void RadGridView1_DataLoaded(object sender, EventArgs e)
        {
            foreach (var col in RadGridView1.Columns)
            {
                int index = RadGridView1.Columns.IndexOf(col);
                if (index < 6 || index == RadGridView1.Columns.Count - 1)
                    col.IsReadOnly = true;
            }
        }

        private void btnAllocate_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.Allocate();
            foreach (var col in RadGridView1.Columns)
            {
                int index = RadGridView1.Columns.IndexOf(col);
                if (index >= 6 && index < RadGridView1.Columns.Count - 1)
                {
                    var on = col.Header.ToString();
                    if (on.Contains("order"))
                    {
                        col.IsVisible = false;
                        continue;
                    }
                    //col.AggregateFunctions.Add(new SumFunction { ResultFormatString = "{0}件", SourceField = on, SourceFieldType = typeof(int?) });
                    //内存中动态生成一个XAML，描述了一个DataTemplate
                    XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                    XElement xGrid = new XElement(ns + "Grid", new XElement(ns + "Grid.ColumnDefinitions", new XElement(ns + "ColumnDefinition", new XAttribute("Width", "Auto")), new XElement(ns + "ColumnDefinition", new XAttribute("Width", "Auto"))));
                    xGrid.Add(new XElement(ns + "TextBlock", new XAttribute("Text", "{Binding Path=" + on + "}"), new XAttribute("Margin", "0 0 5 0")));
                    xGrid.Add(
                        new XElement(ns + "TextBlock", new XAttribute("Foreground", "Red"), new XAttribute("Text", "{Binding Path=" + on + "order}"), new XAttribute("FontSize", "8"), new XAttribute("Grid.Column", "1")));

                    XElement xDataTemplate = new XElement(ns + "DataTemplate", new XAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
                    xDataTemplate.Add(xGrid);
                    XmlReader xr = xDataTemplate.CreateReader();
                    DataTemplate dataTemplate = XamlReader.Load(xr) as DataTemplate;
                    col.CellTemplate = dataTemplate;
                }
            }
        }

        private void RadGridView1_CellEditEnded(object sender, Telerik.Windows.Controls.GridViewCellEditEndedEventArgs e)
        {
            if (e.EditAction == Telerik.Windows.Controls.GridView.GridViewEditAction.Commit)
            {
                int quaOld = 0, quaNew = 0;
                if (e.OldData != DBNull.Value)
                {
                    quaOld = Convert.ToInt32(e.OldData);
                }
                if (e.NewData != DBNull.Value)
                {
                    quaNew = Convert.ToInt32(e.NewData);
                }
                DataRowView row = e.Cell.ParentRow.DataContext as DataRowView;
                row["剩余可用库存"] = Convert.ToInt32(row["剩余可用库存"]) - (quaNew - quaOld);
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
