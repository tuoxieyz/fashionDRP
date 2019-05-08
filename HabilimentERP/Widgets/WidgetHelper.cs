using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;

namespace HabilimentERP
{
    /// <summary>
    /// 部件附加类
    /// 窃以为是附加属性、附加事件的典型应用
    /// </summary>
    public class WidgetHelper
    {
        #region 附加属性

        public static readonly DependencyProperty StateProperty =
               DependencyProperty.RegisterAttached("State",
               typeof(WidgetState),
               typeof(WidgetHelper),
               new FrameworkPropertyMetadata(new PropertyChangedCallback(OnStateChanged)));

        public static WidgetState GetState(DependencyObject obj)
        {
            return (WidgetState)obj.GetValue(StateProperty);
        }

        public static void SetState(DependencyObject obj, WidgetState value)
        {
            obj.SetValue(StateProperty, value);
        }

        private static void OnStateChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var widget = o as UserControl;
            if (widget != null)
            {
                RoutedEventArgs arg = new RoutedEventArgs(WidgetHelper.StateChangedEvent, new { Widget = widget, e.NewValue, e.OldValue });
                widget.RaiseEvent(arg);
            }
        }

        #endregion

        #region 附加事件,交由他人触发

        public static readonly RoutedEvent StateChangedEvent = EventManager.RegisterRoutedEvent("StateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WidgetHelper));

        public static void AddStateChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.AddHandler(WidgetHelper.StateChangedEvent, handler);
            }
        }

        public static void RemoveStateChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(WidgetHelper.StateChangedEvent, handler);
            }
        }

        #endregion
    }
}
