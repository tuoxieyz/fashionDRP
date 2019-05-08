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
using VersionManager.BO;
using Telerik.Windows.Controls.Data.DataForm;
using Telerik.Windows.Controls;
using View.Extension;

namespace VersionManager
{
    /// <summary>
    /// Interaction logic for SoftVersionList.xaml
    /// </summary>
    public partial class SoftVersionList : UserControl
    {
        public SoftVersionList()
        {
            InitializeComponent();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            SoftVersionTrackBO version = btn.DataContext as SoftVersionTrackBO;
            SoftVersionCUWin win = new SoftVersionCUWin();
            win.DataContext = version;
            win.Owner = UIHelper.GetAncestor<Window>(this);
            win.ShowDialog();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("删除版本信息将同时删除该版本与客户的对应关系,\n请确认是否真的要删除?", "注意", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                RadButton btn = sender as RadButton;
                SoftVersionTrackBO version = btn.DataContext as SoftVersionTrackBO;
                var result = version.Soft.Delete(version);
                MessageBox.Show(result.Message);
            }
        }
    }
}
