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
using System.Windows.Media.Animation;
using System.ComponentModel;
using View.Extension;


namespace HabilimentERP
{
    /// <summary>
    /// InfoCalendar.xaml 的交互逻辑
    /// </summary>
    public partial class InfoCalendar : UserControl, IWidget
    {
        #region 辅助类

        //没办法，只能是public，作为内嵌类甚至不能是internal
        [ValueConversion(typeof(string), typeof(BitmapImage))]
        public class TabStringToImageCvt : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string uri = value.ToString();
                uri = uri.Substring(0, uri.Length - 5) + parameter.ToString() + ".png";
                return new BitmapImage(new Uri(uri));
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public static TabStringToImageCvt TabImgCvt = new TabStringToImageCvt();

        private Pan _pan = new Pan();
        private Grip _grip = new Grip();

        private bool _isFirstLoad = true;//是否第一次加载，避免重复注册事件

        public InfoCalendar()
        {
            InitializeComponent();

            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                this.Loaded += delegate
                  {
                      var template = (ControlTemplate)((Setter)calendar.CalendarItemStyle.Setters[1]).Value;
                      //CalendarItem为calendar模板中的一个元素，名称为PART_CalendarItem，它应用了calendar.CalendarItemStyle
                      var header = (FrameworkElement)calendar.Template.FindName("PART_CalendarItem", calendar);
                      _pan.Invest(this, (FrameworkElement)template.FindName("header", header));

                      Border tabContainBorder = (Border)tc.Template.FindName("tabContainBorder", tc);
                      tabContainBorder.MinWidth = tc.ActualWidth;
                      tabContainBorder.MinHeight = tc.ActualHeight;

                      _grip.Invest(tabContainBorder, (FrameworkElement)tabContainBorder.FindName("GripPath"));

                      Button tabCloseBtn = (Button)tabContainBorder.FindName("tabCloseBtn");
                      tabCloseBtn.Click += delegate
                      {
                          Storyboard sb = (Storyboard)((FrameworkElement)(tabContainBorder.Parent)).Resources["sbInfoPanelFadeOut"];

                          sb.Completed += delegate
                          {
                              calendar.Focus();
                              tc.SelectedIndex = -1;
                              tabContainBorder.Width = tabContainBorder.MinWidth;
                              tabContainBorder.Height = tabContainBorder.MinHeight;
                              tabContainBorder.Visibility = Visibility.Hidden;
                          };
                          sb.Begin();
                      };

                      tc.SelectedIndex = -1;
                      tc.SelectionChanged += delegate
                      {

                          Storyboard sb = (Storyboard)((FrameworkElement)(tabContainBorder.Parent)).Resources["sbInfoPanelFadeIn"];

                          if (tabContainBorder.Visibility == Visibility.Hidden && tc.SelectedIndex != -1)
                          {
                              tabContainBorder.Visibility = Visibility.Visible;
                              sb.Begin();
                          }
                      };

                      tabContainBorder.DragOver += new DragEventHandler(tabContainBorder_DragOver);

                      this.Unloaded += delegate {
                          //RaiseEvent可在外部触发非本类的事件
                          tabCloseBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                      };
                  };
            }

            spPad.PreviewMouseLeftButtonDown += spPad_PreviewMouseLeftButtonDown;
            spPad.PreviewMouseMove += spPad_PreviewMouseMove;
        }

        private void PART_MinButton_Click(object sender, RoutedEventArgs e)
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
                    WidgetHelper.SetState(this, value);
                    OnPropertyChanged("State");//加入State是依赖项属性则本身就具有通知特性
                }
            }
        }

        public BitmapImage Icon
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/Images/BottomBar/date.png", UriKind.Relative));
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

        #region 拖放操作

        private bool _isDragging;
        private Point _startPoint;
        private FrameworkElement _dragScope;
        private VisualAdorner _overlayElement;
        private UIElement _originalElement;

        private void spPad_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(spPad);
            _originalElement = spPad; //e.Source as UIElement;
            spPad.CaptureMouse();
        }

        private void spPad_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if ((_isDragging == false) && ((Math.Abs(e.GetPosition(spPad).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(spPad).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                {
                    DragStarted(new Scratchpad { Width = 170, Height = 180 });
                }
            }
        }

        private void DragStarted(UIElement visual)
        {
            _isDragging = true;
            this._dragScope = this.Parent as FrameworkElement;

            _overlayElement = new VisualAdorner(_originalElement, visual);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_originalElement);
            layer.Add(_overlayElement);

            DragEventHandler draghandler = (ss, ee) =>
            {
                if (this._overlayElement != null)
                {
                    this._overlayElement.LeftOffset = ee.GetPosition(spPad).X;
                    this._overlayElement.TopOffset = ee.GetPosition(spPad).Y;
                }
            };
            this._dragScope.PreviewDragOver += draghandler;//注意这代替了原先的MouseMove，此处以_dragScope为拖放容器，因此避免了Mouse移到控件外时Move事件不再触发的问题
            DataObject data = new DataObject(typeof(FrameworkElement), visual);
            DragDrop.DoDragDrop(visual, data, DragDropEffects.Move);//注意这代替了原先的MouseMove，此处将阻塞直到拖放操作完成

            AdornerLayer.GetAdornerLayer(_overlayElement.AdornedElement).Remove(_overlayElement);

            _overlayElement = null;
            _isDragging = false;
            this._dragScope.PreviewDragOver -= draghandler;

            Mouse.Capture(null);
        }

        //在tabContainBorder上拖动时不产生数据转移
        void tabContainBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        #endregion

        private void PART_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.State = WidgetState.Closed;
        }
    }
}
