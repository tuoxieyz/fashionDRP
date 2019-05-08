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

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for SetGoodReturnMoneyWin.xaml
    /// </summary>
    public partial class SetGoodReturnMoneyWin : Window
    {
        internal event Action<decimal> ReturnMoneySettedEvent;

        public SetGoodReturnMoneyWin()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var money = inputReturnMoney.Value;
            if (money == null)
            {
                MessageBox.Show("请输入退货金额.");
                return;
            }
            if (money < 0)
            {
                MessageBox.Show("退货金额不能为负数.");
                return;
            }
            if (ReturnMoneySettedEvent != null)
            {
                ReturnMoneySettedEvent(money.Value);
            }
            this.DialogResult = true;
            this.Close();
        }
    }
}
