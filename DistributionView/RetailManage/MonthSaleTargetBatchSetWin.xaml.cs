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
using Telerik.Windows.Controls;
using DistributionViewModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for MonthSaleTargetBatchSetWin.xaml
    /// </summary>
    public partial class MonthSaleTargetBatchSetWin : Window
    {
        internal event Action SetCompleted;

        public MonthSaleTargetBatchSetWin()
        {
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MonthSaleTargetBatchSetVM _dataContext = layoutGrid.DataContext as MonthSaleTargetBatchSetVM;
            var result = _dataContext.Save();
            MessageBox.Show(result.Message);
            if (result.IsSucceed)
            {
                if (SetCompleted != null)
                    SetCompleted();
                this.Close();
            }
        }
    }
}
