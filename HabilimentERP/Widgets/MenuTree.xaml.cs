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
using HabilimentERP.Gestures;
using System.ComponentModel;
using Telerik.Windows;
using Telerik.Windows.Controls;
using SysProcessModel;
using SysProcessViewModel;
using View.Extension;

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for MenuTree.xaml
    /// </summary>
    public partial class MenuTree : UserControl, IWidget
    {
        WatermarkTextBox _tbQSearch;

        /// <summary>
        /// 叶子节点单击事件
        /// </summary>
        public event Action<SysModule> ShowModuleEvent;

        public MenuTree()
        {
            InitializeComponent();

            _tbQSearch = new WatermarkTextBox("快速查找菜单项")
            {
                Margin = new Thickness(0),
                BorderThickness = new Thickness(0),
                Background = null,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            _tbQSearch.TextChanged += new TextChangedEventHandler(tbQSearch_TextChanged);

            bdQuikSearch.Child = _tbQSearch;
            popSearch.PlacementTarget = _tbQSearch;

            //new Pan().Invest(this, header);//可能RenderTransform会被其余代码覆盖（在MenuTree实例化后又指定了RenderTransform）
            var pan = new Pan();
            this.Loaded += delegate { pan.Invest(this, header); };
            this.Unloaded += delegate { pan.UnInvest(header); };
        }

        void tbQSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            MenuTreeVM vm = (MenuTreeVM)this.DataContext;
            string qs = _tbQSearch.Text.Trim();
            if (!string.IsNullOrEmpty(qs))
            {
                vm.QSearchCommand.Execute(qs);
                if (lvSearch.Items.Count == 0)
                    popSearch.IsOpen = false;
                else
                    popSearch.IsOpen = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.State = WidgetState.Closed;
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.State = WidgetState.Mined;
        }

        public WidgetState State
        {
            get
            {
                return WidgetHelper.GetState(this);
            }
            set
            {
                if (value != State)
                {
                    if (State == WidgetState.Closed && value == WidgetState.Showed)//重新打开，刷新
                    {
                        this._tbQSearch.Clear();
                    }
                    WidgetHelper.SetState(this, value);
                    OnPropertyChanged("State");
                }
            }
        }

        public BitmapImage Icon
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/Images/BottomBar/menu.png", UriKind.Relative));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        bool _lockInBar = true;

        public bool LockInBar
        {
            get
            {
                return _lockInBar;
            }
            set
            {
                _lockInBar = value;
            }
        }

        private void tvMenu_ItemClick(object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem item = (RadTreeViewItem)e.OriginalSource;
            if (!item.HasItems && ShowModuleEvent != null) //是叶子节点
            {
                ShowModuleEvent(((ModuleTreeItem)item.DataContext).Module);
            }
        }

        private void lvSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = e.AddedItems;
            if (list.Count == 1)
            {
                QSModuleTreeItem qm = list[0] as QSModuleTreeItem;
                if (qm != null && ShowModuleEvent != null)
                    ShowModuleEvent(qm.Module);
            }
        }
    }
}
