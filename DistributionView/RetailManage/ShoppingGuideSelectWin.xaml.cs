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
using Telerik.Windows.Data;
using Telerik.Windows.Controls.GridView;
using DistributionModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for ShoppingGuideSelectWin.xaml
    /// </summary>
    public partial class ShoppingGuideSelectWin : Window
    {
        RetailShoppingGuideVM _dataContext = new RetailShoppingGuideVM();

        public event Action<RetailShoppingGuide> ShoppingGuideSelected;

        public ShoppingGuideSelectWin()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            RadGridView1.RowActivated += new EventHandler<RowEventArgs>(RadGridView1_RowActivated);
        }

        void RadGridView1_RowActivated(object sender, RowEventArgs e)
        {
            RetailShoppingGuide o = e.Row.Item as RetailShoppingGuide;
            if (o != null && ShoppingGuideSelected != null)
            {
                ShoppingGuideSelected(o);
                this.Close();
            }
        }
    }
}
