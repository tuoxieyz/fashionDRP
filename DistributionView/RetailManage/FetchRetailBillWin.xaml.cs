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
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for FetchRetailBillWin.xaml
    /// </summary>
    public partial class FetchRetailBillWin : Window
    {
        internal event Action<HoldRetailEntity> FetchRetailEvent;

        public FetchRetailBillWin()
        {
            InitializeComponent();
        }

        private void btnFetch_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            var entity = (HoldRetailEntity)btn.DataContext;
            if (FetchRetailEvent != null)
                FetchRetailEvent(entity);
            ObservableCollection<HoldRetailEntity> context = this.DataContext as ObservableCollection<HoldRetailEntity>;
            context.Remove(entity);
            this.Close();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<HoldRetailEntity> context = this.DataContext as ObservableCollection<HoldRetailEntity>;
            RadButton btn = sender as RadButton;
            context.Remove((HoldRetailEntity)btn.DataContext);
        }
    }
}
