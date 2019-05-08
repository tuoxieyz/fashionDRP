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
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using DistributionModel.Finance;
using SysProcessViewModel;
using SysProcessView;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for ReceiveMoney.xaml
    /// </summary>
    public partial class ReceiveMoney : UserControl
    {
        DataFormCommandButtonsVisibility _access;
        VoucherReceiveMoneyVM _dataContext = new VoucherReceiveMoneyVM();

        public ReceiveMoney()
        {
            this.DataContext = _dataContext;
            //var list = context.Query.LinqOP.Search<BusiDataDictionary>(o => o.ParentCode == "02" && o.IsEnabled).ToList();
            var list = VMGlobal.DistributionQuery.LinqOP.Search<VoucherItemKind>(o => o.Kind == 2 && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            this.Resources.Add("itemKinds", list);
            var enabledList = list.FindAll(o => o.IsEnabled);
            this.Resources.Add("enabledItemKinds", enabledList);

            InitializeComponent();
            _access = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.财务收款);
            myRadDataForm.CommandButtonsVisibility = _access;
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                if (e.ItemPropertyDefinition.PropertyName == "BrandID")
                {
                    cbx.ItemsSource = VMGlobal.PoweredBrands;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<VoucherReceiveMoney>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VoucherReceiveMoney>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_CurrentItemChanged(object sender, EventArgs e)
        {
            VoucherReceiveMoney dm = (VoucherReceiveMoney)myRadDataForm.CurrentItem;
            if (dm != null)
            {
                if (dm.Status)
                    myRadDataForm.CommandButtonsVisibility = _access ^ DataFormCommandButtonsVisibility.Edit ^ DataFormCommandButtonsVisibility.Delete;
                else
                    myRadDataForm.CommandButtonsVisibility = _access;
            }
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VoucherReceiveMoney dm = (VoucherReceiveMoney)myRadDataForm.CurrentItem;
            if (dm.Status)
            {
                MessageBox.Show("不能修改已审核单据");
                e.Cancel = true;
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
