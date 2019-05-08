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

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for LoginUI.xaml
    /// </summary>
    public partial class LoginUI : UserControl
    {
        internal event Action<string, string> Login;

        public LoginUI()
        {
            InitializeComponent();
            this.Loaded += delegate
            {
                txtUserCode.Focus();
                txtUserCode.SelectAll();
            };
        }

        internal void ClearPassword()
        {
            txtPassword.Clear();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Login != null)
            {
                try
                {
                    Login(txtUserCode.Text.Trim(), txtPassword.Password);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("登录时发生异常,信息:" + ex.Message);
                }
            }
        }
    }
}
