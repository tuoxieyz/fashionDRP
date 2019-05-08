using DistributionModel;
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
    /// StoringReturnGoodReject.xaml 的交互逻辑
    /// </summary>
    public partial class StoringReturnGoodReject : UserControl
    {
        StoringReturnGoodRejectVM _dataContext = new StoringReturnGoodRejectVM();

        public StoringReturnGoodReject()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (BillGoodReturnForSearch)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = ReportDataContext.SearchBillDetails<BillGoodReturnDetails>(item.ID);
            }
        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var entity = (BillGoodReturnForSearch)btn.DataContext;            
            var result = _dataContext.Storing(entity);
            MessageBox.Show(result.Message);
        }
    }
}
