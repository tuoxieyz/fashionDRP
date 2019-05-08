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
using DistributionModel;
using DistributionView.RetailManage;

namespace DistributionView
{
    /// <summary>
    /// Interaction logic for ShoppingGuideSelector.xaml
    /// </summary>
    public partial class ShoppingGuideSelector : UserControl
    {
        public static readonly DependencyProperty IDValueProperty =
        DependencyProperty.Register("IDValue", typeof(int), typeof(ShoppingGuideSelector), new PropertyMetadata(0));

        public int IDValue
        {
            get { return (int)GetValue(IDValueProperty); }
            set { SetValue(IDValueProperty, value); }
        }

        public ShoppingGuideSelector()
        {
            InitializeComponent();
        }

        private void imgShowWin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShoppingGuideSelectWin win = new ShoppingGuideSelectWin();
            win.ShoppingGuideSelected += new Action<RetailShoppingGuide>(win_ShoppingGuideSelected);
            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.ShowDialog();
        }

        void win_ShoppingGuideSelected(RetailShoppingGuide obj)
        {
            txtCodeName.Text = "[" + obj.Code + "]" + obj.Name;
            IDValue = obj.ID;
        }
    }
}
