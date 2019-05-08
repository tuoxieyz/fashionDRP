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

namespace DistributionView.VIP
{
    /// <summary>
    /// VIPProportion.xaml 的交互逻辑
    /// </summary>
    public partial class VIPProportion : UserControl
    {
        private VIPProportionVM _dataContext = new VIPProportionVM();

        public VIPProportion()
        {
            this.DataContext = _dataContext;
            InitializeComponent();            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            pieKindProportion.ItemsSource = _dataContext.GetKindProportion();
            pieConsumeProportion.ItemsSource = _dataContext.GetConsumeProportion();
            pieActiveProportion.ItemsSource = _dataContext.GetActiveProportion();
        }
    }
}
