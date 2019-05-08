using DistributionModel;
using DistributionViewModel;
using Kernel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using ViewModelBasic;

namespace DistributionView.VIP
{
    /// <summary>
    /// VIPPredepositSetWin.xaml 的交互逻辑
    /// </summary>
    public partial class VIPPredepositSetWin : Window
    {
        private VIPCardBO _vip;

        public VIPPredepositSetWin(VIPCardBO vip)
        {
            _vip = vip;
            InitializeComponent();
            tbName.Text = vip.CustomerName;
            tbBalance.Text = vip.Predeposits.Sum(o => o.StoreMoney + o.FreeMoney - o.ConsumeMoney).ToString("C2");
            txtCode.Focus();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("请扫描输入VIP卡号.");
                return;
            }
            if (txtCode.Text != _vip.Code)
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
            if (txtPassword.Password.ToMD5String() != _vip.PrestorePassword)
            {
                MessageBox.Show("预存密码错误.");
                return;
            }

            VIPPredepositTrack predeposit = this.DataContext as VIPPredepositTrack;
            if (predeposit.StoreMoney == 0)
            {
                MessageBox.Show("充值金额不能为0.");
                return;
            }
            if (string.IsNullOrWhiteSpace(predeposit.Remark))
            {
                predeposit.Remark = "现金预存";
            }
            predeposit.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            predeposit.CreatorID = VMGlobal.CurrentUser.ID;
            predeposit.Kind = true;

            var result = WebApiInvoker.Instance.Invoke<OPResult<VIPPredepositTrack>, VIPPredepositTrack>(predeposit, "BillRetail/SaveVIPPrestore");
            MessageBox.Show(result.Message);

            if (result.IsSucceed)
            {
                predeposit = result.Result;
                _vip.Predeposits.Insert(0, predeposit);
                if (ckPrint.IsChecked.HasValue && ckPrint.IsChecked.Value)
                {
                    this.Print(predeposit);
                }
                this.Close();
            }
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

        private bool Print(VIPPredepositTrack prestore)
        {
            var vp = new VIPPrestorePrintEntity
            {
                CreateTime = prestore.CreateTime,
                Remark = prestore.Remark,
                FreeMoney = prestore.FreeMoney,
                RefrenceBillCode = prestore.RefrenceBillCode,
                ShopAddress = OrganizationListVM.CurrentOrganization.Address,
                ShopName = OrganizationListVM.CurrentOrganization.Name,
                StoreMoney = prestore.StoreMoney,
                VIPCode = _vip.Code,
                VIPName = _vip.CustomerName
            };
            decimal balance = 0;
            decimal.TryParse(tbBalance.Text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CreateSpecificCulture("zh-CN"), out balance);
            balance += prestore.StoreMoney + prestore.FreeMoney;
            vp.Balance = balance;
            try
            {
                View.Extension.UIHelper.Print("PreStorePrintTemplate.xaml", vp);
            }
            catch (Exception ex)
            {
                MessageBox.Show("打印凭证出错:" + ex.Message);
                return false;
            }
            return true;
        }

        #region 辅助类

        private class VIPPrestorePrintEntity
        {
            private string _header = "预存凭证";
            /// <summary>
            /// 票头
            /// </summary>
            public string Header { get { return _header; } set { _header = value; } }

            public string RefrenceBillCode { get; set; }
            /// <summary>
            /// VIP姓名
            /// </summary>
            public string VIPName { get; set; }

            public string VIPCode { get; set; }
            public decimal StoreMoney { get; set; }
            public decimal FreeMoney { get; set; }
            /// <summary>
            /// 当前余额
            /// </summary>
            public decimal Balance { get; set; }
            public string Remark { get; set; }
            public string ShopName { get; set; }
            public DateTime CreateTime { get; set; }
            public string CreateTimeString
            {
                get
                {
                    return CreateTime.ToString("yyyy-MM-dd HH:mm");
                }
            }
            public string ShopAddress { get; set; }
        }

        #endregion
    }
}
