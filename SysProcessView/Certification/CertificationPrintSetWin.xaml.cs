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
using System.Windows.Shapes;
using System.Printing;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Barcode;
using SysProcessViewModel;
using telerik = Telerik.Windows.Controls;
using SysProcessView;
using System.Data;
using SysProcessModel;
using ERPViewModelBasic;
using Model.Extension;

namespace SysProcessView.Certification
{
    /// <summary>
    /// Interaction logic for CertificationPrintSetWin.xaml
    /// </summary>
    public partial class CertificationPrintSetWin : Window
    {
        //internal event Action<CertificationPrintTicket> SetCompletedEvent;
        CertificationPrintVM _certificationPrintVM = new CertificationPrintVM();

        public CertificationPrintSetWin()
        {
            InitializeComponent();
            panelPrint.DataContextChanged += delegate
            {
                this.ConstructQuantityGrid();
            };
            btnPrint.Click += new RoutedEventHandler(PrintWithGridViewQuantity);
        }

        public CertificationPrintSetWin(List<CertificationBO> certs, IEnumerable<ProductShow> products, List<ProductUniqueCodeMapping> pumapping = null)
        {
            InitializeComponent();
            tbPrintQua.Visibility = gvDatas.Visibility = System.Windows.Visibility.Collapsed;
            btnPrint.Click += (sender, e) =>
            {
                RadButton btn = (RadButton)sender;
                PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                var printDialog = new PrintDialog();
                printDialog.PrintQueue = defaultPrintQueue;
                int quaSuccessful = 0;
                foreach (ProductShow product in products)
                {
                    var cert = certs.Find(o => o.StyleCode == product.StyleCode);
                    if (cert != null)
                    {
                        cert.Quantity = product.Quantity;
                        cert.Color = string.Format("[{0}]{1}", product.ColorCode, VMGlobal.Colors.Find(o => o.Code == product.ColorCode).Name);
                        cert.Size = string.Format("[{0}]{1}", VMGlobal.Sizes.Find(o => o.Name == product.SizeName).Code, product.SizeName);
                        panelPrint.DataContext = cert;
                        try
                        {
                            this.Print(product.ProductID,
                                printDialog,
                                pumapping == null ? Enumerable.Range(1, product.Quantity).Select(o => product.ProductCode).ToList() : pumapping.Where(o => o.ProductID == product.ProductID).Select(o => o.UniqueCode).ToList(),
                                ref quaSuccessful);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("打印出现异常,错误信息:" + ex.Message);
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show("款式" + product.StyleCode + "的合格证信息不存在.");
                    }
                }
                MessageBox.Show("合格证打印并保存成功" + quaSuccessful + "份");
            };
        }

        private void Print(int pid, PrintDialog printDialog, IEnumerable<string> barCodes, ref int quaSuccessful)
        {
            var paginator = new CertificationPaginator(panelPrint, barCodes)
            {
                PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight)//此处可能需要进行缩放处理
            };
            try
            {
                printDialog.PrintDocument(paginator, "合格证");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (paginator.PageNumber > 0)
                {
                    quaSuccessful += paginator.PageNumber;
                }
            }
        }

        private void PrintWithGridViewQuantity(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
            var printDialog = new PrintDialog();
            printDialog.PrintQueue = defaultPrintQueue;
            //var copies = ((CertificationPrintTicket)btn.DataContext).Copies;
            //var barCodes = _productUniqueCodeMappingVM.GenerateBarCodes(copies);
            CertificationBO certification = panelPrint.DataContext as CertificationBO;
            var dv = gvDatas.ItemsSource as DataView;
            int quaSuccessful = 0;
            foreach (DataRowView row in dv)
            {
                for (int i = 1; i < dv.Table.Columns.Count; i++)
                {
                    int qua = 0;
                    int.TryParse(row[i].ToString(), out qua);
                    if (qua > 0)
                    {
                        string ccode = row[0].ToString();
                        string sname = dv.Table.Columns[i].ColumnName;
                        certification.Color = string.Format("[{0}]{1}", ccode, VMGlobal.Colors.Find(o => o.Code == ccode).Name);
                        certification.Size = string.Format("[{0}]{1}", VMGlobal.Sizes.Find(o => o.Name == sname).Code, sname);
                        certification.Quantity = qua;
                        var product = _certificationPrintVM.GetProduct(certification.StyleID, ccode, sname);
                        if (product != null)
                        {
                            var barCodes =
#if UniqueCode
                            _certificationPrintVM.GenerateBarCodes(qua);
#else
 Enumerable.Range(1, qua).Select(o => product.Code).ToList();
#endif
                            try
                            {
                                this.Print(product.ID, printDialog, barCodes, ref quaSuccessful);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("打印出现异常,错误信息:" + ex.Message);
                                goto Finally;
                            }
                        }
                    }
                }
            }
        Finally:
            MessageBox.Show("合格证打印并保存成功" + quaSuccessful + "份");

            //var paginator = new CertificationPaginator(panelPrint, gvDatas.ItemsSource as DataView, _productUniqueCodeMappingVM)
            //{
            //    PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight)//此处可能需要进行缩放处理
            //};
            //if (paginator.PageCount == 0)
            //{
            //    MessageBox.Show("请输入打印份数.");
            //    return;
            //}
            //try
            //{
            //    printDialog.PrintDocument(paginator, "合格证");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("打印出现异常,错误信息:" + ex.Message);
            //}
            //finally
            //{
            //    if (paginator.PageNumber > 1)
            //    {
            //        CertificationBO certification = panelPrint.DataContext as CertificationBO;
            //        var mappings = paginator.BarCodes.Take(paginator.PageNumber).Select(o => new ProductUniqueCodeMapping { ProductID = certification.StyleID, UniqueCode = o });
            //        var result = _productUniqueCodeMappingVM.AddMappings(mappings);
            //        if (result.IsSucceed)
            //        {
            //            MessageBox.Show("合格证成功打印" + paginator.PageNumber + "份");
            //            if (paginator.PageNumber == paginator.PageCount)
            //                this.Close();
            //        }
            //        else
            //            MessageBox.Show(result.Message);
            //    }
            //}
            //printDialog.PrintVisual(panelPrint, "测试");
        }

        private void Symbology_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            barCode.Content = null;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                switch (e.AddedItems[0].ToString())
                {
                    case "Code128":
                        barCode.Content = new RadBarcode128();
                        break;
                    case "Code93":
                        barCode.Content = new RadBarcode93();
                        break;
                    case "Code39":
                        barCode.Content = new RadBarcode39();
                        break;
                }
            }
            if (barCode.Content != null && barCode.Content is SingleSectionBarcodeBase)
            {
                Binding binding = new Binding();
                binding.Source = barCode.DataContext;
                binding.Path = new PropertyPath("BarCode");
                binding.Mode = BindingMode.TwoWay;
                ((SingleSectionBarcodeBase)barCode.Content).SetBinding(SingleSectionBarcodeBase.TextProperty, binding);
            }
        }

        private void ConstructQuantityGrid()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("ColorCode", typeof(string)));

            CertificationBO certification = panelPrint.DataContext as CertificationBO;
            var style = certification.Style;
            var sizes = style.Sizes.OrderBy(o => o.Code).ToList();
            foreach (var sn in sizes)
            {
                table.Columns.Add(new DataColumn(sn.Name, typeof(int)));
                gvDatas.Columns.Add(new telerik::GridViewDataColumn() { Header = sn.Name, UniqueName = sn.Name, DataMemberBinding = new Binding(sn.Name) });
            }
            var colors = style.Colors.OrderBy(o => o.Code).ToList();
            foreach (var cc in colors)
            {
                //gvDatas.Items.Add(new QuantitySetForProStyle { ColorCode = cc.Code });
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                row["ColorCode"] = cc.Code;
            }
            gvDatas.ItemsSource = table.DefaultView;//一定要用DataView，否则不能编辑,shit
            //gvDatas.BeginEdit();//默认为第一个能编辑的cell
        }

        private class CertificationPaginator : DocumentPaginator
        {
            private int _pageCount;
            private FrameworkElement _printPanel;

            /// <summary>
            /// 打印生成的条码集合
            /// </summary>
            private IEnumerable<string> _barCodes;

            /// <summary>
            /// 当前打印第几页
            /// </summary>
            public int PageNumber { get; private set; }

            public CertificationPaginator(FrameworkElement printPanel, IEnumerable<string> barCodes)
            {
                _printPanel = printPanel;
                var certification = _printPanel.DataContext as CertificationBO;
                _pageCount = certification.Quantity;
                _barCodes = barCodes;
            }

            //private void SetColorSize()
            //{
            //    int temp = 0;
            //    foreach (DataRowView row in _dv)
            //    {
            //        for (int i = 1; i < _dv.Table.Columns.Count; i++)
            //        {
            //            int qua = 0;
            //            bool flag = int.TryParse(row[i].ToString(), out qua);
            //            if (flag)
            //            {
            //                temp += qua;
            //                if (temp >= PageNumber)
            //                {
            //                    var certification = _printPanel.DataContext as CertificationBO;
            //                    string ccode = row[0].ToString();
            //                    certification.Color = string.Format("[{0}]{1}", ccode, VMGlobal.Colors.Find(o => o.Code == ccode).Name);
            //                    string sname = _dv.Table.Columns[i].ColumnName;
            //                    certification.Size = string.Format("[{0}]{1}", VMGlobal.Sizes.Find(o => o.Name == sname).Code, sname);
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}

            public override DocumentPage GetPage(int pageNumber)
            {
                PageNumber = pageNumber + 1;
                var certification = _printPanel.DataContext as CertificationBO;
                //这样赋值，由于wpf默认是单线程，因此界面上的值不会马上刷新，打印出来的还是原值,调用Measure和Arrange也不行
                certification.BarCode = _barCodes.ElementAt(pageNumber);
                View.Extension.UIHelper.DoEvents();//强制刷新UI
                //_pageCount = 5;//可以这样设置以取消后续打印(此处为第5页之后不再打印)
                return new DocumentPage(_printPanel);
            }

            public override bool IsPageCountValid
            {
                get { return true; }
            }

            public override int PageCount
            {
                get { return _pageCount; }
            }

            public override Size PageSize
            {
                get;
                set;
            }

            public override IDocumentPaginatorSource Source
            {
                get { return null; }
            }
        }
    }

    public class CertificationPrintTicket
    {
        //private int _copies = 20;
        //public int Copies { get { return _copies; } set { _copies = value; } }

        private string _symbology = "Code128";
        public string Symbology
        {
            get
            {
                return _symbology;
            }
            set
            {
                _symbology = value;
            }
        }

        private string[] _symbologies = new string[] { "Code128", "Code93", "Code39" };
        public IEnumerable<string> Symbologies
        {
            get
            {
                return _symbologies;
            }
        }

        private int _width = 170;
        public int Width { get { return _width; } set { _width = value; } }
        public int _height = 326;
        public int Height { get { return _height; } set { _height = value; } }

        public CertificationPrintTicket()
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            var sizes = lp.Search<BusiDataDictionary>(o => o.Name == "合格证尺寸");
            var children = lp.GetDataContext<BusiDataDictionary>();
            var query = from child in children
                        from size in sizes
                        where child.ParentCode == size.Code
                        select child;
            var data = query.ToList();
            var width = data.FirstOrDefault(o => o.Name == "宽度");
            var height = data.FirstOrDefault(o => o.Name == "高度");
            if (width != null)
            {
                int widtht;
                int.TryParse(width.Value, out widtht);
                _width = widtht;
            }
            if (height != null)
            {
                int heightt;
                int.TryParse(height.Value, out heightt);
                _height = heightt;
            }
        }
    }

    //public class BarCodeTemplateSelector : DataTemplateSelector
    //{
    //    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    {
    //        string symbology = item.ToString();

    //        switch (symbology)
    //        {
    //            case "Code128":
    //                return Code128Template;
    //            case "Code93":
    //                return Code93Template;
    //            case "Code39":
    //                return Code39Template;
    //        }

    //        return base.SelectTemplate(item, container);
    //    }

    //    public DataTemplate Code128Template { get; set; }
    //    public DataTemplate Code93Template { get; set; }
    //    public DataTemplate Code39Template { get; set; }
    //}
}
