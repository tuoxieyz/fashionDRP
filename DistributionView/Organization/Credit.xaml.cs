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
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using SysProcessViewModel;
using SysProcessView;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Organization
{
    /// <summary>
    /// Credit.xaml 的交互逻辑
    /// </summary>
    public partial class Credit : UserControl
    {
        OrganizationCreditVM _dataContext = new OrganizationCreditVM();

        public Credit()
        {
            var dataContext = _dataContext;
            this.Resources.Add("context", dataContext);
            InitializeComponent();

            myRadDataForm.CommandButtonsVisibility = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.机构资信);
            colRaise.IsVisible = (VMGlobal.CurrentUser.OperateAccess & BasicInfoEnum.机构资信) == BasicInfoEnum.机构资信;
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbxBrand = (RadComboBox)e.Editor;
                cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            }
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<OrganizationCredit>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<OrganizationCredit>(myRadDataForm, _dataContext, e);
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }

        private void btnRaise_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(btn);
            row.IsSelected = true;
            OrganizationCredit credit = (OrganizationCredit)btn.DataContext;
            if (credit.ID != default(int))
            {
                CreditRaiseWin win = new CreditRaiseWin();
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                win.SettingEvent += (increase,ee) => {
                    var result = _dataContext.RaiseCredit(credit, increase);
                    MessageBox.Show(result.Message);
                    ee.Handled = result.IsSucceed;                        
                };
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("请先保存资信明细.");
            }
        }
    }
}
