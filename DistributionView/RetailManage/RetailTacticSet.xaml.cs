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
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using SysProcessModel;
using DistributionViewModel.Retail;
using SysProcessView;
using SysProcessViewModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for RetailTacticSet.xaml
    /// </summary>
    public partial class RetailTacticSet : UserControl
    {
        RetailTacticVM _dataContext = new RetailTacticVM();
        DataFormCommandButtonsVisibility _access;

        public RetailTacticSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();

            _access = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.零售策略);
            myRadDataForm.CommandButtonsVisibility = _access;
            //if (_access != DataFormCommandButtonsVisibility.All)
            //    RadGridView1.Columns["colOperate"].IsVisible = false;
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                SysProcessView.UIHelper.SetAPTForFilter(e.ItemPropertyDefinition.PropertyName, cbx);
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<RetailTactic>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<RetailTactic>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RetailTactic tactic = (RetailTactic)myRadDataForm.CurrentItem;
            if (tactic.OrganizationID != VMGlobal.CurrentUser.OrganizationID)
            {
                MessageBox.Show("只能修改本机构创建的零售策略.");
                e.Cancel = true;
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            RetailTactic tactic = (RetailTactic)btn.DataContext;
            bool showOnly = tactic.OrganizationID != VMGlobal.CurrentUser.OrganizationID;
            var styleIDs = VMGlobal.DistributionQuery.LinqOP.Search<RetailTacticProStyleMapping, int>(selector: o => o.StyleID, condition: o => o.TacticID == tactic.ID).ToArray();
            StyleSelectWin win = new StyleSelectWin(tactic.BrandID, showOnly: showOnly, styleIDsSeleted: styleIDs);
            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.SetCompleted += delegate(IEnumerable<ProStyle> styles)
            {
                var mapping = styles.Select(o => new RetailTacticProStyleMapping { TacticID = tactic.ID, StyleID = o.ID });
                var result = _dataContext.SetStylesForTactic(tactic.ID, mapping);
                if (result.IsSucceed)
                {
                    MessageBox.Show("设置成功");
                    win.Close();
                }
                else
                    MessageBox.Show(result.Message);
            };
            win.ShowDialog();
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            RetailTactic tactic = (RetailTactic)myRadDataForm.CurrentItem;
            tactic.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
