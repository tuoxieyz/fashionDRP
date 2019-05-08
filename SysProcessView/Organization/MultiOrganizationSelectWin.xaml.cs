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
using System.Windows.Shapes;
using SysProcessViewModel;
using SysProcessModel;
using System.Globalization;
using System.Collections;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for MultiOrganizationSelectWin.xaml
    /// </summary>
    public partial class MultiOrganizationSelectWin : Window
    {
        public event Action SetCompleted;
        OrganizationSelectVM _dataContext = null;

        public MultiOrganizationSelectWin(OrganizationSelectVM dataContext)
        {
            _dataContext = dataContext;
            this.DataContext = _dataContext;
            InitializeComponent();

            //if (_dataContext.DefaultOrSelectedOrganizations.Count() != 0)
            //{
            //不放到Loaded中就不行(默认项未选中)，只能说坑爹
            this.Loaded += delegate
            {
                RadGridView1_Filtered(null, null);
                _dataContext.PropertyChanged += _dataContext_PropertyChanged;
            };
            //}
        }

        void _dataContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Entities")
            {
                RadGridView1_Filtered(null, null);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SetCompleted != null)
            {
                SetCompleted();
            }
            this.Close();
        }

        private void cbToggleAll_Click(object sender, RoutedEventArgs e)
        {
            //if (_dataContext.Entities != null)
            //{
            //    CheckBox cb = sender as CheckBox;
            //    bool check = cb.IsChecked.Value;
            //    CompositeFilterDescriptorCollection filters = radTreeListView.FilterDescriptors as CompositeFilterDescriptorCollection;
            //    if (filters != null && filters.Count > 0)
            //    {
            //        var condition = filters.GetFilter<OrganizationForSelect>();
            //        _dataContext.SelectOrganizationByCondition(condition);
            //    }
            //}
            if (radTreeListView.Items.Count > 0)
            {
                CheckBox cb = sender as CheckBox;
                bool check = cb.IsChecked.Value;
                radTreeListView.ExpandAllHierarchyItems();
                foreach (OrganizationForSelect item in radTreeListView.Items)
                {
                    item.SelectState = check ? SelectStateEnum.Selected : SelectStateEnum.UnSelected;
                }
            }
        }

        private void ResetToggleAllState()
        {
            if (radTreeListView.Items.Count > 0)
            {
                var roots = _dataContext.Entities.Where(o => radTreeListView.Items.Contains(o));
                if (roots.All(o => o.SelectState == SelectStateEnum.Selected))
                    cbToggleAll.IsChecked = true;
                else
                    cbToggleAll.IsChecked = false;
            }
        }

        private void RadGridView1_Filtered(object sender, Telerik.Windows.Controls.GridView.GridViewFilteredEventArgs e)
        {
            //cbToggleAll.IsChecked = true;
            //foreach (OrganizationForSelect item in radTreeListView.Items)
            //{
            //    if (item.IsSelectedNew == null || !item.IsSelectedNew.Value)
            //    {
            //        cbToggleAll.IsChecked = false;
            //        return;
            //    }
            //}
            if (radTreeListView.Items.Count > 0)
            {
                var roots = _dataContext.Entities.Where(o => radTreeListView.Items.Contains(o)).ToList();
                radTreeListView.ExpandAllHierarchyItems();
                foreach (OrganizationForSelect item in roots)
                {
                    //var row = radTreeListView.ItemContainerGenerator.ContainerFromItem(item) as RadTreeViewItem;
                    ResetOrganizationState(item);
                }
                ResetToggleAllState();
            }
        }

        private void ResetOrganizationState(OrganizationForSelect organization)
        {
            if (organization.ChildrenOrganizations != null && organization.ChildrenOrganizations.Count() > 0)
            {
                var children = organization.ChildrenOrganizations.Where(o => radTreeListView.Items.Contains(o)).ToList();
                if (children.Count > 0)
                {
                    children.ForEach(o => ResetOrganizationState(o));
                    var flag = (organization.SelectState == SelectStateEnum.Selected || organization.SelectState == SelectStateEnum.SelfSelected);//原先是否被选中
                    if (children.All(o => o.SelectState == SelectStateEnum.Selected))
                        organization.SelectState = flag ? SelectStateEnum.Selected : SelectStateEnum.SelfUnSelected;
                    else if (children.Any(o => o.SelectState != SelectStateEnum.UnSelected))
                        organization.SelectState = flag ? SelectStateEnum.SelfSelected : SelectStateEnum.SelfUnSelected;
                    else
                        organization.SelectState = flag ? SelectStateEnum.SelfSelected : SelectStateEnum.UnSelected;
                }
            }
        }

        private void checkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CheckBox cb = e.OriginalSource as CheckBox;
            if (cb != null)
            {
                OrganizationForSelect organization = cb.DataContext as OrganizationForSelect;
                var state = cb.IsChecked.Value ? SelectStateEnum.Selected : SelectStateEnum.UnSelected;
                ExpandRowAsRecursion(organization, state);
                var root = _dataContext.GetRootOrganization(organization);
                if (root != null)
                    ResetOrganizationState(root);
                ResetToggleAllState();
            }
        }

        private void ExpandRowAsRecursion(OrganizationForSelect organization, SelectStateEnum state)
        {
            organization.SelectState = state;
            if (organization.ChildrenOrganizations != null && organization.ChildrenOrganizations.Count() > 0)
            {
                radTreeListView.ExpandHierarchyItem(organization);
                foreach (OrganizationForSelect item in organization.ChildrenOrganizations)
                {
                    //var row = radTreeListView.ItemContainerGenerator.ContainerFromItem(item) as RadTreeViewItem;
                    if (radTreeListView.Items.Contains(item))
                        ExpandRowAsRecursion(item, state);
                }
            }
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid cb = sender as Grid;
            OrganizationForSelect organization = cb.DataContext as OrganizationForSelect;
            if (organization.SelectState == SelectStateEnum.UnSelected || organization.SelectState == SelectStateEnum.SelfUnSelected)
                organization.SelectState = SelectStateEnum.SelfSelected;
            else
                organization.SelectState = SelectStateEnum.SelfUnSelected;
            var root = _dataContext.GetRootOrganization(organization);
            if (root != null)
                ResetOrganizationState(root);
            ResetToggleAllState();
        }
    }

    //public class ChildrenOrganizationToCheckStateCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        IEnumerable<OrganizationForSelect> organizations = value as IEnumerable<OrganizationForSelect>;
    //        if (organizations != null && organizations.Count() > 0)
    //        {
    //            //if (organizations.All(o => o.IsSelectedNew))
    //            return true;
    //        }
    //        return false;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class ThreeCheckStateToSelectedCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        bool? isChecked = (bool?)value;
    //        if (isChecked == null)//呈第三种状态时表示未选中
    //        {
    //            return false;
    //        }
    //        return isChecked.Value;
    //    }
    //}

    public class SelectStateToCheckStateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SelectStateEnum state = (SelectStateEnum)value;
            if (state == SelectStateEnum.SelfSelected || state == SelectStateEnum.Selected)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //bool isChecked = (bool)value;
            //return isChecked ? SelectStateEnum.Selected : SelectStateEnum.UnSelected;
            return null;
        }
    }

    public class SelectStateToVisibilityCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SelectStateEnum state = (SelectStateEnum)value;
            if (state == SelectStateEnum.SelfSelected || state == SelectStateEnum.SelfUnSelected)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            return isChecked ? SelectStateEnum.Selected : SelectStateEnum.UnSelected;
        }
    }
}
