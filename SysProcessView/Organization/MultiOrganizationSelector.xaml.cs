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
using SysProcessViewModel;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for MultiOrganizationSelector.xaml
    /// </summary>
    public partial class MultiOrganizationSelector : UserControl
    {
        OrganizationSelectVM _dataContext;

        private bool _canUserToggleShowAllLower;

        /// <summary>
        /// 用户能否选择是否显示跨级下级
        /// </summary>
        public bool CanUserToggleShowAllLower
        {
            get { return _canUserToggleShowAllLower; }
            set
            {
                if (_canUserToggleShowAllLower != value)
                {
                    _canUserToggleShowAllLower = value;
                    btnClearSelectedOrganizations.ToolTip = "清除已选机构";
                    if (value)
                    {
                        btnClearSelectedOrganizations.ToolTip += "(多次点击切换跨级机构可见性)";
                    }
                }
            }
        }

        public static readonly DependencyProperty ShowAllLowerProperty =
        DependencyProperty.Register("ShowAllLower", typeof(bool), typeof(MultiOrganizationSelector), new PropertyMetadata(false, null, new CoerceValueCallback(OnShowAllLowerCoerced)));

        /// <summary>
        /// 是否显示所有下级机构（包括跨级机构）
        /// </summary>
        public bool ShowAllLower
        {
            get { return (bool)GetValue(ShowAllLowerProperty); }
            set { SetValue(ShowAllLowerProperty, value); }
        }

        private bool _filterCurrent = true;

        /// <summary>
        /// 是否过滤掉当前机构
        /// </summary>
        public bool FilterCurrent
        {
            get { return _filterCurrent; }
            set { _filterCurrent = value; }
        }

        /// <summary>
        /// 是否只显示店铺
        /// </summary>
        public bool IsShowShopOnly { get; set; }

        public static readonly DependencyProperty SelectedOrganizationArrayProperty =
        DependencyProperty.Register("SelectedOrganizationArray", typeof(IEnumerable<SysOrganization>), typeof(MultiOrganizationSelector), new PropertyMetadata(null));

        public IEnumerable<SysOrganization> SelectedOrganizationArray
        {
            get { return (IEnumerable<SysOrganization>)GetValue(SelectedOrganizationArrayProperty); }
            set { SetValue(SelectedOrganizationArrayProperty, value); }
        }

        public MultiOrganizationSelector()
        {
            InitializeComponent();//注意UI上的属性赋值逻辑在InitializeComponent中执行，因此我将OrganizationSelectVM的构造置于InitializeComponent之后     

            bool isLoaded = false;
            //以上说法错误
            //UI上的属性赋值逻辑在构造函数后执行，因此我将OrganizationSelectVM的构造置于Loaded事件中             
            this.Loaded += delegate
            {
                if (!isLoaded)
                {
                    _dataContext = new OrganizationSelectVM(FilterCurrent, ShowAllLower, CanUserToggleShowAllLower, IsShowShopOnly);
                    var binding = new Binding();
                    binding.Source = _dataContext;
                    binding.Path = new PropertyPath("ShowAllLower");
                    binding.Mode = BindingMode.TwoWay;
                    this.SetBinding(MultiOrganizationSelector.ShowAllLowerProperty, binding);

                    //直接放在构造函数中不能反馈到绑定源，放在Loaded事件中可以，不知原因
                    SelectedOrganizationArray = _dataContext.DefaultOrSelectedOrganizations;//多处重复代码，只是为了客户端能收到通知，还有其它好的方式么？
                    isLoaded = true;
                }
            };
        }

        private void btnSelectOrganizations_Click(object sender, RoutedEventArgs e)
        {
            MultiOrganizationSelectWin win = new MultiOrganizationSelectWin(_dataContext);
            win.SetCompleted += delegate()
            {
                SelectedOrganizationArray = _dataContext.DefaultOrSelectedOrganizations;
                if (_dataContext.DefaultOrSelectedOrganizations.Count() == 0)
                {
                    tbOrganizations.Clear();
                }
                else
                {
                    string info = "";
                    foreach (var o in _dataContext.DefaultOrSelectedOrganizations)
                    {
                        info += o.Name + ",";
                    }
                    tbOrganizations.Text = info.TrimEnd(',');
                }
            };

            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.ShowDialog();
        }

        private void btnClearSelectedOrganizations_Click(object sender, RoutedEventArgs e)
        {
            _dataContext.ClearSelected();
            SelectedOrganizationArray = _dataContext.DefaultOrSelectedOrganizations;
            if (CanUserToggleShowAllLower)
            {
                ShowAllLower = !ShowAllLower;
            }
            tbOrganizations.Clear();
        }

        private static object OnShowAllLowerCoerced(DependencyObject d, object obj)
        {
            var widget = (MultiOrganizationSelector)d;
            if (widget != null)
            {
                bool flag = (bool)obj;
                widget.tbOrganizations.WatermarkContent = flag ? "留空表示所有机构(包括跨级子级)" : "留空表示所有机构(不包括跨级子级)";
                widget.SelectedOrganizationArray = widget._dataContext.DefaultOrSelectedOrganizations;
            }
            return obj;
        }
    }
}
