using DistributionViewModel;
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
using Telerik.Windows.Controls;

namespace DistributionView.Bill
{
    /// <summary>
    /// AuditingGoodReturnForSubordinate.xaml 的交互逻辑
    /// </summary>
    public partial class AuditingGoodReturnForSubordinate : UserControl
    {
        AuditingGoodReturnForSubordinateVM _dataContext;

        public AuditingGoodReturnForSubordinate()
        {
            _dataContext = new AuditingGoodReturnForSubordinateVM();
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (BillGoodReturnForSearch)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = item.Details;
            }
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillGoodReturnForSearch)((RadButton)sender).DataContext;
            var opresult = _dataContext.Reject(item);
            MessageBox.Show(opresult.Message);
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            var item = (BillGoodReturnForSearch)((RadButton)sender).DataContext;
            var opresult = _dataContext.Auditing(item);
            MessageBox.Show(opresult.Message);
        }
    }
}
