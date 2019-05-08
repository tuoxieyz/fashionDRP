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

namespace DistributionView.RetailManage
{
    /// <summary>
    /// ShopExpensesSet.xaml 的交互逻辑
    /// </summary>
    public partial class ShopExpensesSet : UserControl
    {
        ShopExpensesSetVM _dataContext = new ShopExpensesSetVM();

        public ShopExpensesSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RadDatePicker datePicker = (RadDatePicker)sender;
                DateTime date = (DateTime)e.AddedItems[0];
                datePicker.DateTimeText = date.ToString("yyyy-MM");
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RadGridView1.CommitEdit();
            Image btn = (Image)sender;
            var result = _dataContext.Save((ShopExpensesBO)btn.DataContext);
            MessageBox.Show(result.Message);
        }
    }
}
