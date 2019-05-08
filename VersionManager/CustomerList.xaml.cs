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
using VersionManager.ViewModel;
using Telerik.Windows.Controls.Data.DataForm;

using View.Extension;
using ViewModelBasic;

namespace VersionManager
{
    /// <summary>
    /// Interaction logic for CustomerList.xaml
    /// </summary>
    public partial class CustomerList : UserControl,IRefresh
    {
        public CustomerList()
        {
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            CustomerListVM dataContext = this.DataContext as CustomerListVM;
            UIHelper.AddOrUpdateRecord(myRadDataForm, dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var confirm = MessageBox.Show("删除客户信息将同时删除该客户和相关软件的对应关系,\n请确认是否真的要删除?", "注意", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                CustomerListVM dataContext = this.DataContext as CustomerListVM;
                UIHelper.DeleteRecord(myRadDataForm, dataContext, e);
            }
        }

        public void Refresh()
        {
            this.DataContext = new CustomerListVM();
        }
    }
}
