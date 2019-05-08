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
using DistributionModel;
using DistributionViewModel;
using SysProcessModel;
using SysProcessViewModel;
using System.Globalization;
using System.IO;
using System.Windows.Markup;
using Kernel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for CashWin.xaml
    /// </summary>
    public partial class CashWin : Window
    {
        BaseBillRetailVM _retailVM = null;
        decimal _vipPreBalance = 0;

        internal event Action CashScceedEvent;

        public CashWin(BaseBillRetailVM dataVM)
        {
            InitializeComponent();
            _retailVM = dataVM;
            this.DataContext = _retailVM;
            this.Loaded += delegate
            {
                inputTakeMoney.Focus();
                inputTakeMoney_ValueChanged(null, null);
                if (_retailVM.VipBO == null)
                    bdPrestore.Visibility = System.Windows.Visibility.Collapsed;
                else
                {
                    _vipPreBalance = VMGlobal.DistributionQuery.LinqOP.Search<VIPPredepositTrack>(o => o.VIPID == _retailVM.VipBO.CardInfo.ID).Sum(o => o.StoreMoney + o.FreeMoney - o.ConsumeMoney);
                    if (_vipPreBalance <= 0)
                        bdPrestore.Visibility = System.Windows.Visibility.Collapsed;
                    else
                        tbPredeposit.Text = string.Format("{0:C2}", _vipPreBalance);
                }
            };
        }

        private void inputTakeMoney_ValueChanged(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            SetBackMoney();
        }

        private void SetBackMoney()
        {
            if (inputTakeMoney.Value == null)
                inputTakeMoney.Value = 0;
            if (inputPredepositPay.Value == null)
                inputPredepositPay.Value = 0;
            var retail = _retailVM.Master;
            retail.ReceiveMoney = retail.CostMoney - inputPredepositPay.Value.Value - retail.TicketMoney;
            tbBackMoney.Text = string.Format("{0:C2}", inputTakeMoney.Value.Value - retail.ReceiveMoney);
        }

        private void btnCash_Click(object sender, RoutedEventArgs e)
        {
            if (_retailVM.VipBO != null)
            {
                if (_retailVM.Master.PredepositPay > 0)
                {
                    if (string.IsNullOrEmpty(_retailVM.VipBO.CardInfo.PrestorePassword))
                    {
                        MessageBox.Show("请先设置预存密码.");
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtCode.Text))
                    {
                        MessageBox.Show("请扫描输入VIP卡号.");
                        return;
                    }
                    if (txtCode.Text != _retailVM.VipBO.CardInfo.Code)
                    {
                        MessageBox.Show("请输入正确的卡号.");
                        txtCode.Clear();
                        _timeInputCode = null;
                        txtCode.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        MessageBox.Show("请输入预存密码.");
                        return;
                    }
                    if (txtPassword.Password.ToMD5String() != _retailVM.VipBO.CardInfo.PrestorePassword)
                    {
                        MessageBox.Show("预存密码错误.");
                        return;
                    }
                }
            }
            var result = _retailVM.Save();
            if (!result.IsSucceed)
            {
                MessageBox.Show(result.Message);
                return;
            }
            try
            {
                PrintFromFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("打印或备份小票出错:" + ex.Message);
                return;
            }
            if (CashScceedEvent != null)
                CashScceedEvent();
            this.Close();
        }

        private void Print(FixedPage page)
        {
            var retail = _retailVM.Master;
            var details = _retailVM.Details;

            var cp = new CashPrintEntity
            {
                RetailCode = retail.Code,
                CreateTime = retail.CreateTime,
                ReceiveTicket = retail.ReceiveTicket,
                TicketMoney = retail.TicketMoney,
                CostMoney = retail.CostMoney,
                PredepositPay = retail.PredepositPay,
                Cashier = VMGlobal.CurrentUser.Name,
                Quantity = retail.Quantity,
                Remark = retail.Remark
            };
            if (inputTakeMoney.Value != null)
            {
                cp.TakeMoney = inputTakeMoney.Value.Value;
                cp.BackMoney = cp.TakeMoney - retail.ReceiveMoney;
            }
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var shop = lp.Search<ViewOrganization>(o => o.ID == retail.OrganizationID).First();
            cp.ShopName = shop.Name;
            cp.ShopAddress = shop.Address;
            if (retail.GuideID != null)
            {
                var guide = lp.Search<RetailShoppingGuide, string>(selector: o => o.Name, condition: o => o.ID == retail.GuideID.Value).FirstOrDefault();
                cp.Guide = guide;
            }
            if (retail.VIPID != null)
            {
                var vip = lp.Search<VIPCard>(o => o.ID == retail.VIPID.Value).FirstOrDefault();
                if (vip != null)
                {
                    cp.VIPName = vip.CustomerName + (vip.Sex ? "先生" : "女士");
                    cp.PredepositBalance = lp.Search<VIPPredepositTrack>(o => o.VIPID == vip.ID).Sum(o => o.StoreMoney + o.FreeMoney - o.ConsumeMoney);
                }
            }
            cp.ProductCollection = details.Select(o =>
            {
                var d = (BillRetailDetailsForPrint)o;
                return new CashPrintProduct { 单价 = d.Price, 折后价 = d.Price * d.Quantity * d.Discount / 100.0M, 数量 = d.Quantity, SKU码 = d.ProductCode, 折扣 = d.Discount };
            });
            page.DataContext = cp;
            CashPrintHelper.SaveXPS(page, cp.RetailCode);
            if (ckPrint.IsChecked.Value)
            {
                View.Extension.UIHelper.Print(page);
            }
        }

        private void PrintFromResource()
        {
            //直接将CashPrintTemplate.xaml拷贝到Debug目录，然后去load是不行的。项目手记第157条
            //这么写似乎是Content在资源管理器中的相对路径作为参数，其实是Pack语法的简写方式，假如当前运行程序是本程序集那么这么写就不会产生错误，如果是其它程序集那么需要参照以下的写法
            //Uri printTemplate = new Uri("/PrintTemplates/CashPrintTemplate.xaml", UriKind.Relative);
            //当前运行程序不是本程序集这么写是错的
            //Uri printTemplate = new Uri("/RetailManage/CashPrintTemplate.xaml", UriKind.Relative);
            //如果以pack开头则认为是绝对路径，下面的是pack语法的相对路径写法（当前程序集非本程序集，此解决方案中运行程序一直都是HabilimentERP.exe，而该资源位于DistributionView.dll程序集中）
            Uri printTemplate = new Uri("/DistributionView;Component/RetailManage/CashPrintTemplate.xaml", UriKind.Relative);
            FixedPage printPage = (FixedPage)Application.LoadComponent(printTemplate);
            this.Print(printPage);
        }

        private void PrintFromFile()
        {
            string printTemplateRepairStatementPath = "CashPrintTemplate.xaml";
            // Define general purpose handlers to be used in browsing the print templates
            FileStream fileStream;

            //FixedDocument doc = new FixedDocument();
            //// A4 Standard: 8.27 x 11.69 inch; 96 dpi
            //Size documentSize = new Size(96 * 8.27, 96 * 11.69);
            //doc.DocumentPaginator.PageSize = documentSize;

            // 1. Reparatur-Bericht
            //  a. Open the filestream
            fileStream = new FileStream(printTemplateRepairStatementPath, FileMode.Open);
            //  b. Read the XAML tree
            FixedPage fixedPage = XamlReader.Load(fileStream) as FixedPage;
            this.Print(fixedPage);

            ////  d. Set the page size
            //fixedPage.Width = doc.DocumentPaginator.PageSize.Width;
            //fixedPage.Height = doc.DocumentPaginator.PageSize.Height;
            ////fixedPage.InvalidateVisual();

            //// Add to document
            //PageContent pageContent = new PageContent();
            //((IAddChild)pageContent).AddChild(fixedPage);
            //doc.Pages.Add(pageContent);
        }

        #region 辅助类

        private class CashPrintEntity
        {
            private string _header = "销售小票";
            /// <summary>
            /// 票头
            /// </summary>
            public string Header { get { return _header; } set { _header = value; } }
            /// <summary>
            /// 零售单编号
            /// </summary>
            public string RetailCode { get; set; }
            /// <summary>
            /// 收银员
            /// </summary>
            public string Cashier { get; set; }
            /// <summary>
            /// VIP姓名
            /// </summary>
            public string VIPName { get; set; }
            /// <summary>
            /// 导购
            /// </summary>
            public string Guide { get; set; }
            /// <summary>
            /// 数量合计
            /// </summary>
            public int Quantity { get; set; }
            public decimal CostMoney { get; set; }
            public int ReceiveTicket { get; set; }
            public decimal TicketMoney { get; set; }
            public decimal PredepositPay { get; set; }
            /// <summary>
            /// 预存款余额
            /// </summary>
            public decimal PredepositBalance { get; set; }
            public decimal TakeMoney { get; set; }
            public decimal BackMoney { get; set; }
            public string Remark { get; set; }
            public string ShopName { get; set; }
            /// <summary>
            /// 收银时间
            /// </summary>
            public DateTime CreateTime { get; set; }
            public string CreateTimeString
            {
                get
                {
                    return CreateTime.ToString("yyyy-MM-dd HH:mm");
                }
            }
            public string ShopAddress { get; set; }

            private string _footer = "请妥善保管好您的小票,谢谢光临.";
            public string Footer { get { return _footer; } set { _footer = value; } }

            public IEnumerable<CashPrintProduct> ProductCollection { get; set; }
        }

        private class CashPrintProduct
        {
            public string SKU码 { get; set; }
            public int 数量 { get; set; }
            public decimal 单价 { get; set; }
            public decimal 折扣 { get; set; }
            public decimal 折后价 { get; set; }
        }

        #endregion

        private void inputPredepositPay_ValueChanged(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            if (inputPredepositPay.Value == null)
                inputPredepositPay.Value = 0;
            //decimal balance = 0;
            ////在xp系统中以下方法返回false，使用decimal.parse方法转换抛出formatexception异常
            //if (decimal.TryParse(tbPredeposit.Text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CreateSpecificCulture("zh-CN"), out balance)
            //    ||
            //    decimal.TryParse(tbPredeposit.Text.Substring(1), out balance)//对可能是xp系统的情况作进一步处理
            //    )
            //{
            //    if (inputPredepositPay.Value.Value > balance)
            //    {
            //        inputPredepositPay.Value = balance;
            //    }
            //}
            if (inputPredepositPay.Value.Value > _vipPreBalance)
                inputPredepositPay.Value = _vipPreBalance;
            //else
            //    inputPredepositPay.Value = 0;
            SetBackMoney();
        }

        private DateTime? _timeInputCode = null;

        private void txtCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_timeInputCode != null)
            {
                var timeTemp = DateTime.Now;
                if ((timeTemp - _timeInputCode).Value.TotalMilliseconds > 60)
                {
                    MessageBox.Show("不允许键盘输入或多次扫描，请一次扫描输入。");
                    txtCode.Clear();
                    _timeInputCode = null;
                    txtCode.Focus();
                    return;
                }
                _timeInputCode = timeTemp;
            }
            else
                _timeInputCode = DateTime.Now;

            if (e.Text != "\r")
            {
                txtCode.Text += e.Text;
            }
        }
    }
}
