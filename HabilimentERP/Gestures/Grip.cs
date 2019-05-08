using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace HabilimentERP.Gestures
{
    public class Grip
    {
        private FrameworkElement _controlToGrip;
        private FrameworkElement _gripHook;
        Point _resizePoint;
        bool _isResizing;

        MouseButtonEventHandler _leftBtnDown;
        MouseEventHandler _mouseMove;
        MouseButtonEventHandler _leftBtnUp;

        public void Invest(FrameworkElement controlToGrip, FrameworkElement gripHook)
        {
            _leftBtnDown = new MouseButtonEventHandler(gripHook_PreviewMouseLeftButtonDown);
            _mouseMove = new MouseEventHandler(gripHook_PreviewMouseMove);
            _leftBtnUp = new MouseButtonEventHandler(gripHook_PreviewMouseLeftButtonUp);

            //没有Preview就不行，估计是事件冒泡时被子元素标记为已处理了
            gripHook.PreviewMouseLeftButtonDown += _leftBtnDown;
            gripHook.PreviewMouseMove += _mouseMove;
            gripHook.PreviewMouseLeftButtonUp += _leftBtnUp;
            _controlToGrip = controlToGrip;
            _gripHook = gripHook;

            if (double.IsNaN(_controlToGrip.Width))
                _controlToGrip.Width = _controlToGrip.ActualWidth;
            if (double.IsNaN(_controlToGrip.Height))
                _controlToGrip.Height = _controlToGrip.ActualHeight;
        }

        public void UnInvest(FrameworkElement gripHook)
        {
            gripHook.PreviewMouseLeftButtonDown -= _leftBtnDown;
            gripHook.PreviewMouseMove -= _mouseMove;
            gripHook.PreviewMouseLeftButtonUp -= _leftBtnUp;
        }

        void gripHook_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isResizing = false;
            _gripHook.ReleaseMouseCapture();
            e.Handled = true;
        }

        void gripHook_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _gripHook.Cursor = Cursors.SizeNWSE;
            if (_isResizing)
            {
                double w, h;
                Point tempResizePoint = e.GetPosition(_gripHook);
                if (_controlToGrip.Width + (w = tempResizePoint.X - _resizePoint.X) >= _controlToGrip.MinWidth)
                    _controlToGrip.Width += w;
                if (_controlToGrip.Height + (h = tempResizePoint.Y - _resizePoint.Y) >= _controlToGrip.MinHeight)
                    _controlToGrip.Height += h;
            }
            e.Handled = true;
        }

        void gripHook_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isResizing = true;
            _resizePoint = e.GetPosition(_gripHook);
            _gripHook.CaptureMouse();
            e.Handled = true;
        }
    }
}
