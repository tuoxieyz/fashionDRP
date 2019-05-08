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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Manufacturing.ViewModel;
using SysProcessView.Certification;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for DeliverySend.xaml
    /// </summary>
    public partial class DeliverySend : UserControl
    {
        BillDeliverySendVM _dataContext = new BillDeliverySendVM();
        CertificationMakeVM _certvm;

        public DeliverySend()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (DeliverySearchEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = item.Details;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var result = _dataContext.Delete((DeliverySearchEntity)btn.DataContext);
            MessageBox.Show(result.Message);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var result = _dataContext.Send((DeliverySearchEntity)btn.DataContext);
            MessageBox.Show(result.Message);
        }

        private void btnPrintCert_Click(object sender, RoutedEventArgs e)
        {
            if (_certvm == null)
                _certvm = new CertificationMakeVM();

            RadButton btn = (RadButton)sender;
            DeliverySearchEntity delivery = (DeliverySearchEntity)btn.DataContext;
            var certs = _dataContext.GetCertifications(delivery, _certvm);
            if (certs.Count > 0)
            {
#if UniqueCode
                var mapping = _dataContext.GetProductUniqueCodeMappings(delivery);
                CertificationPrintSetWin win = new CertificationPrintSetWin(certs, delivery.Details, mapping);
#else
                CertificationPrintSetWin win = new CertificationPrintSetWin(certs, delivery.Details);
#endif

                win.DataContext = new { Certification = certs[0], PrintTicket = new CertificationPrintTicket() };
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("没有找到对应的合格证信息");
            }
        }
    }
}
