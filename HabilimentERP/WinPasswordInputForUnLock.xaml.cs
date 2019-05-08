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
using SysProcessViewModel;

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for WinPasswordInputForUnLock.xaml
    /// </summary>
    public partial class WinPasswordInputForUnLock : Window
    {
        private bool _isUnLock = false;

        public WinPasswordInputForUnLock()
        {
            InitializeComponent();
            this.Loaded += delegate { txtPassword.Focus(); };
            this.Closing += new System.ComponentModel.CancelEventHandler(WinPasswordInputForUnLock_Closing);
        }

        void WinPasswordInputForUnLock_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isUnLock)
                e.Cancel = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var password = txtPassword.Password;
            if (password.ToMD5String() == VMGlobal.CurrentUser.Password)
            {
                _isUnLock = true;
                this.Close();
            }
        }
    }
}
