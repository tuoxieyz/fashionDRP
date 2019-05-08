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
using SysProcessViewModel;
using Telerik.Windows.Controls;
using SysProcessModel;

namespace SysProcessView.Product
{
    /// <summary>
    /// Interaction logic for SKUDeletion.xaml
    /// </summary>
    public partial class SKUDeletion : UserControl
    {
        SKUDeletionVM _dataContext= new SKUDeletionVM();

        public SKUDeletion()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var diaresult = MessageBox.Show("删除SKU码将同时删除订单和生产单等单据中的相关数据,确定删除吗?", "提醒", MessageBoxButton.YesNo);
            if (diaresult == MessageBoxResult.Yes)
            {
                RadButton btn = (RadButton)sender;
                var result = _dataContext.Delete((ViewProduct)btn.DataContext);
                MessageBox.Show(result.Message);
            }
        }
    }
}
