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
using DistributionViewModel;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for StoringProductExchange.xaml
    /// </summary>
    public partial class StoringProductExchange : UserControl
    {
        StoringProductExchangeVM _dataContext = new StoringProductExchangeVM();

        public StoringProductExchange()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (BillStoringProductExchangeEntity)e.Row.Item;
                gv.ItemsSource = item.Details;
            }
        }

        private void btnSendBack_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var result = _dataContext.SendBack((BillStoringProductExchangeEntity)btn.DataContext);
            MessageBox.Show(result.Message);
        }

        private void btnStoring_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var result = _dataContext.Storing((BillStoringProductExchangeEntity)btn.DataContext);
            MessageBox.Show(result.Message);
        }
    }
}
