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
using SysProcessModel;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for OrganizationSelector.xaml
    /// </summary>
    public partial class OrganizationSelector : UserControl
    {
        private bool _isUserEdited = false;//用户是否手动编辑了文本框

        public static readonly DependencyProperty IDValueProperty =
        DependencyProperty.Register("IDValue", typeof(int?), typeof(OrganizationSelector), new PropertyMetadata(null, null, new CoerceValueCallback(OnIDValueCoerced)));

        public int? IDValue
        {
            get { return (int?)GetValue(IDValueProperty); }
            set { SetValue(IDValueProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable<SysOrganization>), typeof(OrganizationSelector));

        public IEnumerable<SysOrganization> ItemsSource
        {
            get
            {
                var source = (IEnumerable<SysOrganization>)GetValue(ItemsSourceProperty);
                if (source == null)
                {
                    source = this.GetOrganizations();
                    SetValue(ItemsSourceProperty, source);
                }
                if (FilterCurrent)
                    source = source.Where(o => o.ID != VMGlobal.CurrentUser.OrganizationID);
                if (IsShowShopOnly)
                    source = source.Where(o => o.Name.EndsWith("店"));
                return source;
            }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty FilterCurrentProperty =
        DependencyProperty.Register("FilterCurrent", typeof(bool), typeof(OrganizationSelector), new PropertyMetadata(false));

        /// <summary>
        /// 是否过滤掉当前机构
        /// </summary>
        public bool FilterCurrent
        {
            get { return (bool)GetValue(FilterCurrentProperty); }
            set { SetValue(FilterCurrentProperty, value); }
        }

        /// <summary>
        /// 是否只显示店铺
        /// </summary>
        public bool IsShowShopOnly { get; set; }

        public OrganizationSelector()
        {
            InitializeComponent();
        }

        private void imgShowWin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ItemsSource.Count() == 0)
            {
                MessageBox.Show("没有可供选择的机构");
                return;
            }
            OrganizationSelectorWin win = new OrganizationSelectorWin(this.ItemsSource);
            win.OrganizationSelected += new Action<SysOrganization>(win_OrganizationSelected);

            win.Closed += delegate
            {

                if (_isUserEdited && IDValue != 0)
                {
                    txtCodeName.Clear();
                    var organization = this.ItemsSource.FirstOrDefault(org => org.ID == IDValue);
                    if (organization != null)
                    {
                        txtCodeName.Text = "[" + organization.Code + "]" + organization.Name;
                        _isUserEdited = false;
                    }
                }
            };

            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.ShowDialog();
        }

        void win_OrganizationSelected(SysOrganization organization)
        {
            txtCodeName.Text = "[" + organization.Code + "]" + organization.Name;
            IDValue = organization.ID;
            _isUserEdited = false;
        }

        private List<SysOrganizationBO> GetOrganizations()
        {
            var orgs = OrganizationListVM.CurrentAndChildrenOrganizations;
            return orgs;
        }

        //Binding导致的属性更改也会触发强制回调
        private static object OnIDValueCoerced(DependencyObject d, object obj)
        {
            var os = (OrganizationSelector)d;
            os.txtCodeName.Text = "";
            var orgs = os.ItemsSource;
            if (obj != null)
            {
                int oid = (int)obj;
                var organization = orgs.FirstOrDefault(org => org.ID == oid);
                if (organization != null)
                {
                    os.txtCodeName.Text = "[" + organization.Code + "]" + organization.Name;
                    os._isUserEdited = false;//私有变量也能这么访问？
                }
            }
            return obj;
        }

        //private static void OnIDValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var os = (OrganizationSelector)d;
        //    var orgs = os.Organizations;
        //    int oid = (int)e.NewValue;
        //    var organization = orgs.FirstOrDefault(org => org.ID == oid);
        //    if (organization != null)
        //    {
        //        os.txtCodeName.Text = "[" + organization.Code + "]" + organization.Name;
        //        os._isUserEdited = false;//私有变量也能这么访问？
        //    }
        //}

        private void txtCodeName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)//回车
            {
                var orgs = this.ItemsSource;
                var orgsFound = orgs.Where(org => org.Code == txtCodeName.Text);
                if (orgsFound != null && orgsFound.Count() == 1)
                {
                    var org = orgsFound.First();
                    IDValue = org.ID;
                }
                else
                {
                    imgShowWin_MouseLeftButtonUp(null, null);
                }
            }
            else
                _isUserEdited = true;
        }

        private void txtCodeName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtCodeName.SelectAll();
        }
    }
}
