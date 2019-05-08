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
using SysProcessViewModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessModel;
using ViewModelBasic;

namespace SysProcessView.SysProcess
{
    /// <summary>
    /// Interaction logic for SysUserSet.xaml
    /// </summary>
    public partial class SysUserSet : UserControl
    {
        public SysUserSet()
        {
            InitializeComponent();
        }

        private void radDataFilter_EditorCreated(object sender, EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "RoleID")
            {
                RadComboBox cbxRole = (RadComboBox)e.Editor;
                cbxRole.ItemsSource = RoleVM.RolesOfCurrentOrgnization;
            }
        }

        private ListBox GetRoleListBox()
        {
            var lbRole = View.Extension.UIHelper.GetDataFormField<ListBox>(myRadDataForm, "lbRole");
            return lbRole;
        }

        private ListBox GetBrandListBox()
        {
            var lbBrand = View.Extension.UIHelper.GetDataFormField<ListBox>(myRadDataForm, "lbBrand");
            return lbBrand;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                SysUserBO user = (SysUserBO)myRadDataForm.CurrentItem;

                var lbBrand = GetBrandListBox();
                var brandSets = lbBrand.ItemsSource as List<HoldableEntity<ProBrand>>;
                user.Brands = brandSets.FindAll(bs => bs.IsHold).Select(bs => bs.Entity).ToList();
                var lbRole = GetRoleListBox();
                var roleSets = lbRole.ItemsSource as List<HoldableEntity<SysRole>>;
                user.Roles = roleSets.FindAll(rs => rs.IsHold).Select(rs => rs.Entity).ToList();

                UserVM context = this.DataContext as UserVM;
                UIHelper.AddOrUpdateRecord<SysUser>(myRadDataForm, context, e);
            }
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("用户不可删除");
            e.Cancel = true;
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SysUser user = (SysUser)myRadDataForm.CurrentItem;
            if (user.ID == VMGlobal.CurrentUser.ID)
            {
                MessageBox.Show("不能修改当前登录用户信息.");
                e.Cancel = true;
            }
        }

        private void btnResetPWD_Click(object sender, RoutedEventArgs e)
        {
            SysUserBO user = (SysUserBO)myRadDataForm.CurrentItem;
            var result = user.ResetPassword();
            MessageBox.Show(result.Message);
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            SysUser user = (SysUser)myRadDataForm.CurrentItem;
            user.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }

        //private void myRadDataForm_CurrentItemChanged(object sender, EventArgs e)
        //{
            //SysUser user = (SysUser)myRadDataForm.CurrentItem;
            //if (myRadDataForm.Mode== RadDataFormMode.AddNew && user.ID != default(int))
            //{
            //    myRadDataForm.CancelEdit();
            //}
        //}
    }
}
