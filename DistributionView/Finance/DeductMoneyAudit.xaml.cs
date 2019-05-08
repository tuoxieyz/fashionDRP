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
using DistributionModel.Finance;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataForm;
using Telerik.Windows.Controls;
using DistributionModel;
using SysProcessViewModel;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Finance
{
    /// <summary>
    /// Interaction logic for DeductMoneyAudit.xaml
    /// </summary>
    public partial class DeductMoneyAudit : UserControl
    {
        VoucherDeductMoneyVM _dataContext = new VoucherDeductMoneyVM(true);

        public DeductMoneyAudit()
        {
            this.DataContext = _dataContext;
            //var list = context.Query.LinqOP.Search<BusiDataDictionary>(o => o.ParentCode == "01" && o.IsEnabled).ToList();
            var list = VMGlobal.DistributionQuery.LinqOP.Search<VoucherItemKind>(o => o.Kind == 1 && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            this.Resources.Add("itemKinds", list);
            var enabledList = list.FindAll(o => o.IsEnabled);
            this.Resources.Add("enabledItemKinds", enabledList);

            InitializeComponent();
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
                    myRadDataForm.CommandButtonsVisibility = DataFormCommandButtonsVisibility.All ^ DataFormCommandButtonsVisibility.Edit ^ DataFormCommandButtonsVisibility.Delete;
                else
                    myRadDataForm.CommandButtonsVisibility = DataFormCommandButtonsVisibility.All;
            }
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(btn);
            row.IsSelected = true;
            var result = _dataContext.Audit((VoucherDeductMoney)btn.DataContext);           
            if (result.IsSucceed)
            {
                RadGridView1.Rebind();
                var fieldStatus = View.Extension.UIHelper.GetDataFormField<DataFormDataField>(myRadDataForm, "fieldStatus");
                var tbStatus = fieldStatus.Content as TextBox;
                BindingExpression be = tbStatus.GetBindingExpression(TextBox.TextProperty);
                be.UpdateTarget();
                MessageBox.Show("审核成功！");
            }
            else
            {
                string msg = result.Message.Replace("更新", "审核");
                MessageBox.Show(msg);
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
    }
}
