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
using SysProcessModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using SysProcessViewModel;
using Telerik.Windows.Controls.Data.DataForm;
using Telerik.Windows.Controls;
using System.Collections;
using System.Windows.Automation;
using DomainLogicEncap;
using System.Globalization;
using ViewModelBasic;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for SysRoleSet.xaml
    /// </summary>
    public partial class SysRoleSet : UserControl
    {
        public SysRoleSet()
        {
            InitializeComponent();
            myRadDataForm.Loaded += delegate
            {
                View.Extension.UIHelper.DisableFormHorizontalScrollBar(myRadDataForm);
            };
        }

        private RadTreeView GetMPTreeView()
        {
            //应将RadDataForm中私有的fieldsContentPresenter封装成公共属性来调用（此处标记）
            var fieldsContentPresenter = myRadDataForm.Template.FindName("PART_FieldsContentPresenter", myRadDataForm) as ContentPresenter;
            var tvModuleProcess = fieldsContentPresenter.ContentTemplate.FindName("tvModuleProcess", fieldsContentPresenter) as RadTreeView;
            return tvModuleProcess;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                List<SysModule> modules = new List<SysModule>();
                RadTreeView tv = this.GetMPTreeView();
                //由于tv.CheckedItems不会包含未展开子节点（即使子节点被选中），因此咱们换一种方式
                foreach (var item in tv.CheckedItems)
                {
                    ModuleTreeItem mti = (ModuleTreeItem)item;
                    modules.Add(mti.Module);
                }
                //以下方式同样木有用
                //IList dataSource = tv.ItemsSource as IList;
                ////dataSource是object集合，但是竟然能用ModuleTreeItem的对象去枚举，第一次知道，不知道其它集合类型会否有类似用法
                //foreach (ModuleTreeItem mti in dataSource)
                //{
                //    RadTreeViewItem container = tv.ContainerFromItemRecursive(mti);
                //    if (container != null && container.CheckState != ToggleState.Off)
                //    {
                //        modules.Add(mti.Module);
                //        if (mti.Children.Count > 0)
                //            modules.AddRange(GetCheckedChildrenModules(tv, mti));
                //    }
                //}                
                var role = (SysRoleBO)myRadDataForm.CurrentItem;
                role.Modules = modules;
                SetOPAndIMAccess(role);
                RoleVM _dataContext = new RoleVM();
                UIHelper.AddOrUpdateRecord<SysRole>(myRadDataForm, _dataContext, e);
                if (myRadDataForm.Mode == RadDataFormMode.AddNew && !e.Cancel)
                {
                    RadGridView1.Rebind();//重新绑定，将创建者反馈到界面上
                }
            }
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var mbResult = MessageBox.Show("确认删除该角色吗？", "提示", MessageBoxButton.OKCancel);
            if (mbResult == MessageBoxResult.OK)
            {
                RoleVM _dataContext = layout.DataContext as RoleVM;
                View.Extension.UIHelper.DeleteRecord<SysRole>(myRadDataForm, _dataContext, e);
            }
        }

        private void tvModuleProcess_Checked(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            bool isInitiallyChecked = (e as RadTreeViewCheckEventArgs).IsUserInitiated;
            if (isInitiallyChecked)//是用户手动选择（非自动Check）
            {
                RadTreeViewItem tvItem = e.OriginalSource as RadTreeViewItem;
                tvItem.ExpandAll();//ExpandAll之后选中项才会出现在CheckedItems中
            }
        }

        private void BindModuleTree(SysRole role)
        {
            if (myRadDataForm.Mode == RadDataFormMode.Edit)
            {
                var modules = RoleLogic.ModuleProcessOfRole(role.ID);
                RadTreeView tv = this.GetMPTreeView();
                IList mtis = tv.ItemsSource as IList;

                foreach (ModuleTreeItem mti in mtis)
                {
                    SetChildrenModulesChecked(modules, mti, tv);
                }
                tv.CollapseAll();//设置好之后全部收缩
            }
        }

        private void SetChildrenModulesChecked(List<SysModule> modules, ModuleTreeItem mti, RadTreeView tv)
        {
            if (mti.Children != null && mti.Children.Count > 0)//判断是否叶子节点
            {
                foreach (ModuleTreeItem m in mti.Children)
                {
                    SetChildrenModulesChecked(modules, m, tv);
                }
            }
            else
            {
                if (modules.Any(sm => sm.ID == mti.Module.ID))
                {
                    RadTreeViewItem tvItem = tv.ContainerFromItemRecursive(mti);
                    if (tvItem != null)
                    {
                        tvItem.CheckState = ToggleState.On;
                    }
                }
            }
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SysRole role = (SysRole)myRadDataForm.CurrentItem;
            if (role.CreatorID != VMGlobal.CurrentUser.ID)
            {
                MessageBox.Show("只能编辑自己创建的角色.");
                e.Cancel = true;
            }
        }

        private void myRadDataForm_CurrentItemChanged(object sender, EventArgs e)
        {
            SysRole role = myRadDataForm.CurrentItem as SysRole;
            if (role != null)
            {
                BindModuleTree(role);
                BindOPAccess(role);
                BindIMAccess(role);
            }
        }

        private void BindOPAccess(SysRole role)
        {
            //if (role.ID == 0)
            //    return;
            ItemsControl op = View.Extension.UIHelper.GetVisualChild<ItemsControl>(myRadDataForm, "opList");
            if (op == null)
                return;
            BasicInfoEnum bi = (BasicInfoEnum)role.OPAccess;

            var fields = op.ChildrenOfType<CheckBox>();
            if (fields != null && fields.Count() > 0)
            {
                foreach (var field in fields)
                {
                    BasicInfoEnum tag = (BasicInfoEnum)field.Tag;
                    if ((bi & tag) == tag)
                        field.IsChecked = true;
                    else
                        field.IsChecked = false;
                }
            }
        }

        private void BindIMAccess(SysRole role)
        {
            ItemsControl op = View.Extension.UIHelper.GetVisualChild<ItemsControl>(myRadDataForm, "imList");
            if (op == null)
                return;
            IMReceiveAccessEnum bi = (IMReceiveAccessEnum)role.IMAccess;

            var fields = op.ChildrenOfType<CheckBox>();
            if (fields != null && fields.Count() > 0)
            {
                foreach (var field in fields)
                {
                    IMReceiveAccessEnum tag = (IMReceiveAccessEnum)field.Tag;
                    if ((bi & tag) == tag)
                        field.IsChecked = true;
                    else
                        field.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// 设置操作权限和IM权限
        /// </summary>
        private void SetOPAndIMAccess(SysRole role)
        {
            role.OPAccess = this.GetAccess("opList");
            role.IMAccess = this.GetAccess("imList");
        }

        private int GetAccess(string controlName)
        {
            ItemsControl accessControl = View.Extension.UIHelper.GetVisualChild<ItemsControl>(myRadDataForm, controlName);
            var fields = accessControl.ChildrenOfType<CheckBox>();
            int tag = 0;
            foreach (var field in fields)
            {
                bool access = field.IsChecked.Value;
                if (access)
                {
                    tag = tag | (int)field.Tag;
                }
            }
            return tag;
        }

        //请看遇到问题第二条,寄希望于RadDataForm有个BeginEdit事件，在那个事件中写逻辑而不是在loaded事件中写逻辑
        private void moduleProcess_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm_CurrentItemChanged(null, null);
        }

        private void newModuleProcess_Loaded(object sender, RoutedEventArgs e)
        {
            SysRole role = myRadDataForm.CurrentItem as SysRole;
            if (role != null)
            {
                ItemsControl op = View.Extension.UIHelper.GetVisualChild<ItemsControl>(myRadDataForm, "imList");
                if (op == null)
                    return;
                var fields = op.ChildrenOfType<CheckBox>();
                if (fields != null && fields.Count() > 0)
                {
                    foreach (var field in fields)
                    {
                        field.IsChecked = true; //默认有权限                        
                    }
                }
            }
        }
    }

    public class RoleOPAccessCvt : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            int opAccess = (int)value;
            EnumVM enumvm = (EnumVM)parameter;
            return enumvm.Values.Where(o => (o.ID & opAccess) == o.ID);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class RoleIMAccessCvt : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            int imAccess = (int)value;
            EnumVM enumvm = (EnumVM)parameter;
            return enumvm.Values.Where(o => (o.ID & imAccess) == o.ID);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
