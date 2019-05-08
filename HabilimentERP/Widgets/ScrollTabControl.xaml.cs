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
using SysProcessModel;
using System.Linq.Expressions;
using View.Extension;

namespace HabilimentERP
{
    /// <summary>
    /// ScrollTabControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollTabControl : UserControl, IWidget//, IDisposable
    {
        private Pan _pan = new Pan();
        private Grip _grip = new Grip();

        public ScrollTabControl()
        {
            InitializeComponent();
            //父控件OnApplyTemplate执行时不代表子控件也OnApplyTemplate
            //所以将以下逻辑放入Loaded节点中
            this.Loaded += delegate
            {
                //下句会诡异地得出tp==null的结果（不定期）
                FrameworkElement tp = (FrameworkElement)tc.Template.FindName("PART_ScrollContentPresenter", tc);
                _pan.Invest(this, tp);
                _grip.Invest(this, GripPath);
            };
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
                    WidgetHelper.SetState(this, value);
                    OnPropertyChanged("State");
                }
            }
        }

        public BitmapImage Icon
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/Images/BottomBar/folder-open.png", UriKind.Relative));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        bool _lockInBar;

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

        public void ReceiveModule(SysModule sm)
        {
            if (tc.HasItems)
            {
                for (int i = 0; i < tc.Items.Count; i++)
                {
                    TabItem item = (TabItem)tc.Items[i];
                    if (item.Tag.ToString() == sm.Code)
                    {
                        tc.Items.MoveCurrentTo(item);
                        return;
                    }
                }
            }

            Type type = Type.GetType(sm.Uri);
            if (type != null)
            {
                var lambda = LambdaExpression.Lambda(System.Linq.Expressions.Expression.New(type));
                var content = lambda.Compile().DynamicInvoke();
                TabItem tabItem = new TabItem { Header = sm.Name, Tag = sm.Code, Content = content }; //使用Code来定位
                tc.Items.Add(tabItem);
                tc.Items.MoveCurrentTo(tabItem);
            }
        }

        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.State = WidgetState.Mined;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.State = WidgetState.Closed;
        }

        //public void Dispose()
        //{
        //    FrameworkElement tp = (FrameworkElement)tc.Template.FindName("PART_ScrollContentPresenter", tc);
        //    _pan.UnInvest(tp);
        //    _pan = null;
        //    _grip.UnInvest(GripPath);
        //    _grip = null;
        //}
    }
}
