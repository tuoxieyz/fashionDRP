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
using Kernel;
using DistributionViewModel;

namespace DistributionView.VIP
{
    /// <summary>
    /// PrestorePasswordSetWin.xaml 的交互逻辑
    /// </summary>
    public partial class PrestorePasswordSetWin : Window
    {
        private VIPCardBO _card = null;

        public PrestorePasswordSetWin(VIPCardBO card)
        {
            InitializeComponent();
            _card = card;
            this.Loaded += delegate { txtCode.Focus(); };
            tbName.Text = card.CustomerName;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("请扫描输入VIP卡号.");
                return;
            }
            if (txtCode.Text != _card.Code)
            {
                MessageBox.Show("请输入正确的卡号.");
                txtCode.Clear();
                _timeInputCode = null;
                txtCode.Focus();
                return;
            }
            string newpwd = txtNewPassword.Password;
            string surepwd = txtNewPasswordSure.Password;
            if (!this.IsMatchPWDReg(newpwd))
            {
                MessageBox.Show("密码必须为至少6位数字.");
                return;
            }
            if (newpwd != surepwd)
            {
                MessageBox.Show("两次输入的密码不一致.");
                return;
            }
            var result = VIPCardVM.SetPrestorePassword(_card, newpwd);
            MessageBox.Show(result.Message);
            if (result.IsSucceed)
                this.Close();
        }

        /// <summary>
        /// 密码规则校验
        /// </summary>
        private bool IsMatchPWDReg(string password)
        {
            string regex = @"^[0-9]{6,}$";
            return password.IsMatch(regex);
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
