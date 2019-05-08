using DistributionModel;
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

namespace DistributionView.Organization
{
    /// <summary>
    /// CreditRaiseWin.xaml 的交互逻辑
    /// </summary>
    public partial class CreditRaiseWin : Window
    {
        internal event Action<int, RoutedEventArgs> SettingEvent;

        public CreditRaiseWin()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtIncrease.Value != null && SettingEvent != null)
                SettingEvent((int)txtIncrease.Value.Value, e);
            if (e.Handled)
                this.Close();
        }
    }
}
