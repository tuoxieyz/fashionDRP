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
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using DistributionViewModel;
using DistributionModel.Finance;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessViewModel;
using SysProcessView;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for DeductMoney.xaml
    /// </summary>
    public partial class DeductMoney : UserControl
    {
        DataFormCommandButtonsVisibility _access;
        VoucherDeductMoneyVM _dataContext = new VoucherDeductMoneyVM();

        public DeductMoney()
        {           
            this.DataContext = _dataContext;
            var list = VMGlobal.DistributionQuery.LinqOP.Search<VoucherItemKind>(o => o.Kind == 1 && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            this.Resources.Add("itemKinds", list);
            var enabledList = list.FindAll(o => o.IsEnabled);
            this.Resources.Add("enabledItemKinds", enabledList);//不知为何，新增或更新好之后属性ItemKindCode将变为null，我估计直接将列表存储为资源给前台绑定会发生这种情况
            //this.Resources.Add("itemKinds", new FinanceVM());//这样也不行
            //后来发现假如直接用telerik:DataFormComboBoxField绑定的话就有这种问题，因此改为以telerik:RadComboBox绑定，具体请看前台xaml
            //但是其它如品牌下拉绑定都不会产生这个问题，难道是因为DataFormComboBoxField的SelectedValuePath的类型只能为int，不能为string吗？待日后研究

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
            SysProcessView.UIHelper.AddOrUpdateRecord<VoucherDeductMoney>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VoucherDeductMoney>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_CurrentItemChanged(object sender, EventArgs e)
        {
            VoucherDeductMoney dm = (VoucherDeductMoney)myRadDataForm.CurrentItem;
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
            VoucherDeductMoney dm = (VoucherDeductMoney)myRadDataForm.CurrentItem;
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
