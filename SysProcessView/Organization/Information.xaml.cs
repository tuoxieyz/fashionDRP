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
using SysProcessViewModel;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.DataForm;
using System.ComponentModel;
using SysProcessModel;
using ViewModelBasic;

namespace SysProcessView.Organization
{
    /// <summary>
    /// Interaction logic for Information.xaml
    /// </summary>
    public partial class Information : UserControl
    {
        public Information()
        {
            InitializeComponent();
            myRadDataForm.CommandButtonsVisibility = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.分支机构);
        }

        private ListBox GetBrandListBox()
        {
            var lbBrand = View.Extension.UIHelper.GetDataFormField<ListBox>(myRadDataForm, "lbBrand");
            return lbBrand;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            //点击取消按钮也会触发该事件，因此此处加了判断
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                var lbBrand = GetBrandListBox();
                var brandSets = lbBrand.ItemsSource as List<HoldableEntity<ProBrand>>;
                SysOrganizationBO org = (SysOrganizationBO)myRadDataForm.CurrentItem;
                org.Brands = brandSets.FindAll(bs => bs.IsHold).Select(bs => bs.Entity).ToList();
                OrganizationListVM context = this.DataContext as OrganizationListVM;
                UIHelper.AddOrUpdateRecord(myRadDataForm, context, e);
            }
        }

        private void myRadDataForm_DeletingItem(object sender, CancelEventArgs e)
        {
            #region 废代码
            //SysOrganization org = (SysOrganization)myRadDataForm.CurrentItem;
            //if (!org.Flag)
            //{
            //    MessageBox.Show("该机构已被禁用！");
            //    e.Cancel = true;//不需在源列表中删除
            //    return;
            //}
            //OrganizationVM context = this.DataContext as OrganizationVM;
            //if (context.Disable(org))
            //{
            //    MessageBox.Show("操作成功！");
            //}
            //else
            //{
            //    MessageBox.Show("操作失败！");
            //}
            //e.Cancel = true;//不需在源列表中删除
            #endregion

            MessageBox.Show("客户信息不可删除.\n若不使用,请将状态设为禁用.");
            e.Cancel = true;
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //不知为何，新增完之后会有空值转换错误信息提示，这里给它强制清空
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
