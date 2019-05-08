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
using DistributionViewModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for VIPUpgradeWin.xaml
    /// </summary>
    public partial class VIPUpgradeWin : Window
    {
        public VIPUpgradeWin()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            VIPUpgradeInfo info = this.DataContext as VIPUpgradeInfo;
            var tactics = info.UpTactics.ToList().FindAll(o => o.IsChecked);
            if (tactics.Count == 0)
            {
                MessageBox.Show("未选择升级选项.");
                return;
            }
            else
            {
                var brandIDs = tactics.Select(o => o.BrandID).Distinct();
                foreach (var bid in brandIDs)
                {
                    if (tactics.Count(o => o.BrandID == bid) > 1)
                    {
                        MessageBox.Show("一个品牌只能选择一个升级选项.");
                        return;
                    }
                }
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            VIPUpgradeInfo info = this.DataContext as VIPUpgradeInfo;
            foreach (var t in info.UpTactics)
                t.IsChecked = false;
        }
    }
}
