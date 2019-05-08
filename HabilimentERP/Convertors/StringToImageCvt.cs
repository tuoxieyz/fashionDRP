using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HabilimentERP.Convertors
{
    /// <summary>
    /// 从String到Image的转换，注意String必须符合WPF的资源定位规则，因为在后台代码中不认相对路径(xaml中认)
    /// 比如：pack://application:,,,/HabilimentERP;component/Images/CalendarInfo/stk1.png
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    internal class StringToImageCvt : IValueConverter
    {        
        /// <param name="parameter"></param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new BitmapImage(new Uri(value.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
