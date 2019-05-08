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
using DistributionModel;
using DomainLogicEncap;
using Telerik.Windows.Controls.Data.DataForm;
using Telerik.Windows.Data;
using DistributionModel.Finance;
using SysProcessViewModel;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for ReceiveMoneyKind.xaml
    /// </summary>
    public partial class ReceiveMoneyKind : UserControl
    {
        VoucherItemKindVM _dataContext = new VoucherItemKindVM(2);

        public ReceiveMoneyKind()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            //myRadDataForm.CommandButtonsVisibility = (myRadDataForm.CommandButtonsVisibility ^ DataFormCommandButtonsVisibility.Delete);//收款方式不得删除
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<VoucherItemKind>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VoucherItemKind>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            VoucherItemKind dmItem = (VoucherItemKind)myRadDataForm.CurrentItem;
            //dmItem.ParentCode = "02";
            //dmItem.Code = CommonDataLogic.GenerateCodeForBusiDataDictionary(dmItem.ParentCode);
            dmItem.Kind = 2;  
            dmItem.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }
}
