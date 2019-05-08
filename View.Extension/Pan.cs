using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace View.Extension
{
    public class Pan
    {
        private FrameworkElement _controlToDrag;
        private bool _isDragging;
        private Point _mousePoint;
        private Vector _removeVector = new Vector(0, 0);
        private FrameworkElement _dragHook;
        private TranslateTransform _translate;

        MouseButtonEventHandler _leftBtnDown;
        MouseEventHandler _mouseMove;
        MouseButtonEventHandler _leftBtnUp;

        /// <summary>
        /// 为FrameworkElement启动拖动功能
        /// </summary>
        /// <param name="controlToDrag">需要拖动功能的组件</param>
        /// <param name="dragHook">拖动锚点</param>
        /// <param name="isAbutCanDrag">是否肯定可被拖动（在组件相互遮盖的情况下标示是否强制拖动被遮盖组件）</param>        
        public void Invest(FrameworkElement controlToDrag, FrameworkElement dragHook, bool isAbutCanDrag = true)
        {
            _leftBtnDown = new MouseButtonEventHandler(dragHook_MouseLeftButtonDown);
            _mouseMove = new MouseEventHandler(dragHook_MouseMove);
            _leftBtnUp = new MouseButtonEventHandler(dragHook_MouseLeftButtonUp);
            if (dragHook != null)
            {
                if (isAbutCanDrag)
                    dragHook.AddHandler(UIElement.MouseLeftButtonDownEvent, _leftBtnDown, true);
                else
                    dragHook.MouseLeftButtonDown += _leftBtnDown;
                dragHook.AddHandler(UIElement.MouseMoveEvent, _mouseMove, true);
                dragHook.AddHandler(UIElement.MouseLeftButtonUpEvent, _leftBtnUp, true);
            }
            _dragHook = dragHook;
            _controlToDrag = controlToDrag;
            _translate = TransformHelper.SetTransform<TranslateTransform>(_controlToDrag);
        }

        public void UnInvest(FrameworkElement dragHook)
        {
            dragHook.RemoveHandler(UIElement.MouseLeftButtonDownEvent, _leftBtnDown);
            dragHook.RemoveHandler(UIElement.MouseMoveEvent, _mouseMove);
            dragHook.RemoveHandler(UIElement.MouseLeftButtonUpEvent, _leftBtnUp);
        }

        void dragHook_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _dragHook.ReleaseMouseCapture();
        }

        void dragHook_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _mousePoint = e.GetPosition(_dragHook);
            //Mouse.Capture(_dragHook, CaptureMode.Element);           
            //不设为true，鼠标将被容器控件即ListBox捕获
            e.Handled = true;
        }

        void dragHook_MouseMove(object sender, MouseEventArgs e)
        {
            //单击按钮时除了MouseDown和MouseUp，竟然也能产生MouseMove事件
            //因此加上e.LeftButton的判断
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                //将鼠标捕获从单击事件移入鼠标移动事件中，这样就避免了由于鼠标被其它控件捕获导致某些控件对单击事件无响应
                _dragHook.CaptureMouse();
                Point tempMousePoint = e.GetPosition(_dragHook);
                _removeVector = tempMousePoint - _mousePoint + _removeVector;
                this.MoveBy(_removeVector);
            }
        }

        private void MoveBy(Vector v)
        {
            _translate.X = v.X;
            _translate.Y = v.Y;
        }
    }
}
