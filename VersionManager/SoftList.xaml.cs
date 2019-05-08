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
using Telerik.Windows.Controls.Data.DataForm;
using VersionManager.BO;
using VersionManager.ViewModel;
using Telerik.Windows.Controls;

using View.Extension;
using ViewModelBasic;

namespace VersionManager
{
    /// <summary>
    /// Interaction logic for SoftList.xaml
    /// </summary>
    public partial class SoftList : UserControl, IRefresh
    {
        public SoftList()
        {
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                var lbxCustomer = UIHelper.GetDataFormField<ListBox>(myRadDataForm, "lbxCustomer");
                var customers = lbxCustomer.ItemsSource as List<CustomerBO>;
                SoftToUpdateBO soft = (SoftToUpdateBO)myRadDataForm.CurrentItem;
                soft.Customers = customers.Where(o => o.IsHold).ToList();
                SoftListVM dataContext = gridLayout.DataContext as SoftListVM;
                UIHelper.AddOrUpdateRecord(myRadDataForm, dataContext, e);
            }
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var confirm = MessageBox.Show("删除软件信息将同时删除该软件与客户的对应关系和已发布的各种版本信息,\n请确认是否真的要删除?", "注意", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                SoftListVM dataContext = gridLayout.DataContext as SoftListVM;
                UIHelper.DeleteRecord(myRadDataForm, dataContext, e);
            }
        }

        private void btnPublic_Click(object sender, RoutedEventArgs e)
        {
            //RadButton btn = sender as RadButton;
            //SoftToUpdateBO soft = btn.DataContext as SoftToUpdateBO;
            //SoftVersionCUWin win = new SoftVersionCUWin();
            //win.DataContext = new SoftVersionTrackBO { Soft = soft };
            //win.Owner = UIHelper.GetAncestor<Window>(this);
            //win.ShowDialog();
        }

        public void Refresh()
        {
            gridLayout.DataContext = new SoftListVM();
            SoftSelectCustomerConvertor convertor = this.Resources["softSelectCustomerConvertor"] as SoftSelectCustomerConvertor;
            convertor.Refresh();
        }
    }
}
