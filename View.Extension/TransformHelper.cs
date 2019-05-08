using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace View.Extension
{
    /// <summary>
    /// Transform帮助类
    /// </summary>
    public static class TransformHelper
    {
        /// <summary>
        /// 为FrameworkElement设置变换
        /// </summary>
        /// <typeparam name="T">变换类型参数</typeparam>
        /// <param name="control">待设置变换的FrameworkElement</param>
        /// <returns>变换对象</returns>
        public static T SetTransform<T>(FrameworkElement control) where T : Transform, new()
        {
            T transform;
            if (control.RenderTransform == null)
                control.RenderTransform = transform = new T();
            else
            {
                transform = FindTransform<T>(control.RenderTransform);
                if (transform == null)
                {
                    transform = new T();
                    var origTransform = control.RenderTransform;
                    var finalTransform = new TransformGroup();
                    finalTransform.Children.Add(origTransform);
                    finalTransform.Children.Add(transform);
                    control.RenderTransform = finalTransform;
                }
            }
            return transform;
        }

        /// <summary>
        /// 在给定Transform查找指定类型的变化对象
        /// </summary>
        /// <typeparam name="T">变换类型参数</typeparam>
        /// <param name="tf">待遍历的Transform</param>
        /// <returns>变换对象</returns>
        public static T FindTransform<T>(Transform tf) where T : Transform, new()
        {
            if (tf is T)
                return (T)tf;
            else if (tf is TransformGroup)
            {
                T ret;
                foreach (var t in ((TransformGroup)tf).Children)
                {                    
                    if ((ret = FindTransform<T>(t)) != null)
                        return ret;
                }
            }
            return null;
        }
    }
}
