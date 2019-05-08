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

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for UniqueCodeTransTrack.xaml
    /// </summary>
    public partial class UniqueCodeTransTrack : UserControl
    {
        public UniqueCodeTransTrack()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUniqueCode.Text))
            {
                MessageBox.Show("请先输入待查询的唯一码");
                return;
            }
            gvDatas.ItemsSource = ReportDataContext.GetUniqueCodeTransTrack(txtUniqueCode.Text.Trim());
        }
    }
}
