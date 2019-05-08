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
using DistributionViewModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Organization
{
    /// <summary>
    /// Interaction logic for StorageInfo.xaml
    /// </summary>
    public partial class StorageInfo : UserControl
    {
        StorageInfoVM _dataContext = new StorageInfoVM();

        public StorageInfo()
        {
            this.DataContext = _dataContext;

            InitializeComponent();
            //Loaded事件会在页签切换时触发，因此会导致侦听方法的反复调用
            //this.Loaded += new RoutedEventHandler(StorageInfo_Loaded);            
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<Storage>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("仓库一旦建立就不允许删除，\n若不使用，请将状态置为禁用。");
            e.Cancel = true;
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            Storage storage = (Storage)myRadDataForm.CurrentItem;
            storage.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }
}
