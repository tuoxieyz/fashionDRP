using DistributionModel;
using DistributionModel.Finance;
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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.DataForm;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Finance
{
    /// <summary>
    /// ReceiveMoneyUnfreeze.xaml 的交互逻辑
    /// </summary>
    public partial class ReceiveMoneyUnfreeze : UserControl
    {
        ReceiveMoneyUnfreezeVM _dataContext = new ReceiveMoneyUnfreezeVM();

        public ReceiveMoneyUnfreeze()
        {
            this.DataContext = _dataContext;

            var list = VMGlobal.DistributionQuery.LinqOP.Search<VoucherItemKind>(o => o.Kind == 2 && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            this.Resources.Add("itemKinds", list);
            var enabledList = list.FindAll(o => o.IsEnabled);
            this.Resources.Add("enabledItemKinds", enabledList);

            InitializeComponent();

            myRadDataForm.CommandButtonsVisibility = DataFormCommandButtonsVisibility.Navigation;
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

        private void btnUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(btn);
            row.IsSelected = true;
            VoucherReceiveMoney dm = (VoucherReceiveMoney)btn.DataContext;
            var result = _dataContext.Unfreeze(dm);
            if (result.IsSucceed)
            {
                RadGridView1.Rebind();
                var fieldFreezenStatus = View.Extension.UIHelper.GetDataFormField<DataFormDataField>(myRadDataForm, "fieldFreezenStatus");
                var tbStatus = fieldFreezenStatus.Content as TextBox;
                BindingExpression be = tbStatus.GetBindingExpression(TextBox.TextProperty);
                be.UpdateTarget();
            }
            MessageBox.Show(result.Message);
        }
    }
}
