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
using System.Collections.ObjectModel;
using ERPViewModelBasic;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for UniqueCodeDetailsTemplate.xaml
    /// </summary>
    public partial class UniqueCodeDetailsTemplate : UserControl
    {
        public UniqueCodeDetailsTemplate()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            ObservableCollection<string> items = icUniqueCode.ItemsSource as ObservableCollection<string>;
            items.Remove((string)btn.DataContext);

            ProductShow product = this.DataContext as ProductShow;
            product.Quantity -= 1;

            var grid = View.Extension.UIHelper.GetAncestor<RadGridView>(this);
            if (grid != null)
                grid.CalculateAggregates();
        }
    }
}
