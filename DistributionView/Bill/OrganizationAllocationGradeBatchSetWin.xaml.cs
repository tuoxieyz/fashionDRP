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

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrganizationAllocationGradeBatchSetWin.xaml
    /// </summary>
    public partial class OrganizationAllocationGradeBatchSetWin : Window
    {
        internal event Action SetCompleted;

        public OrganizationAllocationGradeBatchSetWin()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            OrganizationAllocationGradeBatchSetVM _dataContext = layoutGrid.DataContext as OrganizationAllocationGradeBatchSetVM;
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
