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
using Telerik.Windows.Controls;
using System.Windows.Media.Animation;
using Telerik.Windows;
using SysProcessModel;
using Telerik.Windows.Controls.Docking;
using System.Linq.Expressions;
using System.ServiceModel;
using IWCFService;
using Kernel;
using SysProcessViewModel;
using IWCFServiceForIM;

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for DockingWindow.xaml
    /// </summary>
    public partial class DockingWindow : Window
    {
        RadWatermarkTextBox _tbQSearch;
        MainWindowVM _dataContext = new MainWindowVM();

        public DockingWindow()
        {
            StyleManager.ApplicationTheme = new Windows7Theme();
            this.DataContext = _dataContext;
            InitializeComponent();
            if (_dataContext.CustomerInfo != null)
                this.Title += string.Format("({0}专用版)", _dataContext.CustomerInfo.Name);
            gridContent.Visibility = Visibility.Hidden;
            _dataContext.LogoutEvent += new Action<bool>(_dataContext_LogoutEvent);
            this.Closing += delegate { _dataContext.WindowClosing(); };
        }

        void _dataContext_LogoutEvent(bool isForced)
        {
            gridContent.Visibility = Visibility.Hidden;
            gridLayout.Children.Add(gridLogin);
            loginUI.ClearPassword();
            _tbQSearch = null;
            bdQuikSearch.Child = null;
            popSearch.PlacementTarget = null;
            //tabHost.Items.Clear();
            for (int i = 0; i < dockPanel.Panes.Count(); )
            {
                var item = dockPanel.Panes.ElementAt(i);
                if (item != paneMenu)
                    item.RemoveFromParent();
                else
                    i++;
            }
            foreach (var win in this.OwnedWindows)
            {
                ((Window)win).Close();
            }
            if (isForced)
            {
                this.Dispatcher.Invoke(new Action(() =>
               {
                   //不要忘记this，因为MessageBox是在其它线程创建的
                   MessageBox.Show(this, "您当前使用账号被强制退出.\n可能相同账号在其它地方登录或控制中心要求您下线,若有疑问请联系系统管理员.");
               }));
            }
        }

        private void loginUI_Login(string userCode, string password)
        {
            if (VMGlobal.CurrentUser == null)
            {
                var result = _dataContext.Login(userCode, password);
                if (result.IsSucceed)
                {
                    //new Action(() => RegisterToIM()).BeginInvoke(null, null);                
                    new Action(() => SynchronizeLocalTime()).BeginInvoke(null, null);
                    LoginInit();
                }
                else
                {
                    MessageBox.Show(result.Message);
                }
            }
        }

        /// <summary>
        /// 同步本机时间到服务器时间
        /// </summary>
        private void SynchronizeLocalTime()
        {
            DateTime serverTime = DateTime.Now;
            using (ChannelFactory<IBillService> channelFactory = new ChannelFactory<IBillService>("BillSVC"))
            {
                IBillService service = channelFactory.CreateChannel();
                serverTime = service.GetDateTimeOfServer();
            }
            DateTime localTime = DateTime.Now;
            if (localTime.Date != serverTime.Date)//不在同一天则设置本机时间为服务器时间
            {
                SystemDateTimeManager manager = new SystemDateTimeManager();
                manager.SetLocalDateTime(serverTime);
            }
        }

        private void LoginInit()
        {
            CubicEase cubicEase = new CubicEase();
            Storyboard sb = new Storyboard();
            DoubleAnimation anix = new DoubleAnimation();
            anix.Duration = new Duration(TimeSpan.FromMilliseconds(2000));
            sb.Children.Add(anix);
            anix.To = 1000;
            anix.EasingFunction = cubicEase;
            Storyboard.SetTargetProperty(anix, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            Storyboard.SetTarget(sb, gridLogin);
            sb.Completed += delegate
            {
                gridLayout.Children.Remove(gridLogin);
                gridContent.Visibility = Visibility.Visible;
                _tbQSearch = new RadWatermarkTextBox()
                {
                    Margin = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    Background = null,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    WatermarkContent = "快速查找菜单项"
                };

                _tbQSearch.TextChanged += new TextChangedEventHandler(tbQSearch_TextChanged);

                bdQuikSearch.Child = _tbQSearch;
                popSearch.PlacementTarget = _tbQSearch;
            };
            sb.Begin();
        }

        void tbQSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            MenuTreeVM vm = (MenuTreeVM)dpTree.DataContext;
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

        private void lvSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = e.AddedItems;
            if (list.Count == 1)
            {
                QSModuleTreeItem qm = list[0] as QSModuleTreeItem;
                if (qm != null)
                    ShowModule(qm.Module);
            }
        }

        private void tvMenu_ItemClick(object sender, RadRoutedEventArgs e)
        {
            RadTreeViewItem item = (RadTreeViewItem)e.OriginalSource;
            if (!item.HasItems) //是叶子节点
            {
                ShowModule(((ModuleTreeItem)item.DataContext).Module);
            }
        }

        private void ShowModule(SysModule sm)
        {
            if (tabHost.HasItems)
            {
                for (int i = 0; i < tabHost.Items.Count; i++)
                {
                    RadDocumentPane item = (RadDocumentPane)tabHost.Items[i];
                    if (item.Tag.ToString() == sm.Code)
                    {
                        //tabHost.Items.MoveCurrentTo(item);
                        tabHost.SelectedItem = item;
                        return;
                    }
                }
            }
            if (!string.IsNullOrEmpty(sm.Uri))
            {
                Type type = Type.GetType(sm.Uri);
                if (type != null)
                {
                    var lambda = LambdaExpression.Lambda<Func<object>>(System.Linq.Expressions.Expression.New(type));
                    var content = lambda.Compile()();
                    RadDocumentPane tabItem = new RadDocumentPane { Header = sm.Name, Tag = sm.Code, Content = content, ContextMenuTemplate = null }; //使用Code来定位
                    tabHost.AddItem(tabItem, DockPosition.Center);
                    //tabHost.Items.MoveCurrentTo(tabItem);
                }
            }
        }

        private void menuClose_Click(object sender, RadRoutedEventArgs e)
        {
            var result = MessageBox.Show("确认退出系统吗?", "提示", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void menuLogout_Click(object sender, RadRoutedEventArgs e)
        {
            _dataContext.Logout();
        }

        private void menuPasswordSet_Click(object sender, RadRoutedEventArgs e)
        {
            PasswordSetWin win = new PasswordSetWin();
            win.Owner = this;
            win.ShowDialog();
        }

        private Window GetChildWindow<TWindow>() where TWindow : Window
        {
            foreach (var win in this.OwnedWindows)
            {
                if (win is TWindow)
                {
                    return (Window)win;
                }
            }
            return null;
        }

        //private void versionContrail_Click(object sender, RadRoutedEventArgs e)
        //{
        //    var win = GetChildWindow<VersionContrailWin>();
        //    if (win != null)
        //        win.WindowState = WindowState.Normal;
        //    else
        //    {
        //        win = new VersionContrailWin();
        //        win.Owner = this;
        //        win.Show();
        //    }
        //}

        private void officeSite_Click(object sender, RadRoutedEventArgs e)
        {
            var win = GetChildWindow<WinOfficialWebsite>();
            if (win != null)
                win.WindowState = WindowState.Normal;
            else
            {
                win = new WinOfficialWebsite();
                win.Owner = this;
                try
                {
                    win.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("网站打开失败,信息:" + ex.Message);
                }
            }
        }

        private void menuLock_Click(object sender, RadRoutedEventArgs e)
        {
            WinPasswordInputForUnLock win = new WinPasswordInputForUnLock();
            win.Owner = this;
            win.ShowDialog();
        }

        private void menuRefresh_Click(object sender, RadRoutedEventArgs e)
        {
            VMGlobal.Refresh();
        }

        private void moreMsg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < dockPanel.Panes.Count(); i++)
            {
                var item = dockPanel.Panes.ElementAt(i);
                if (item.Tag != null && item.Tag.ToString() == "IMPane")
                {
                    item.IsHidden = false;
                    item.IsSelected = true;
                    return;
                }
            }
            IMList content = new IMList { DataContext = _dataContext };
            RadDocumentPane pane = new RadDocumentPane { Header = "即时消息", Content = content, ContextMenuTemplate = null, Tag = "IMPane" };
            tabHost.AddItem(pane, DockPosition.Center);
        }
    }
}
