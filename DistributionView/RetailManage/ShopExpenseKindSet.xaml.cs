using DistributionModel;
using DistributionViewModel;
using SysProcessViewModel;
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

namespace DistributionView.RetailManage
{
    /// <summary>
    /// ShopExpenseKindSet.xaml 的交互逻辑
    /// </summary>
    public partial class ShopExpenseKindSet : UserControl
    {
        ShopExpenseKindSetVM _dataContext = new ShopExpenseKindSetVM();

        public ShopExpenseKindSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<ShopExpenseKind>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<ShopExpenseKind>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            ShopExpenseKind kind = (ShopExpenseKind)myRadDataForm.CurrentItem;
            kind.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }
}
