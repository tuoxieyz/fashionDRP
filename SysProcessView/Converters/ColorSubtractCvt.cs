using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace SysProcessView
{
    public class ColorSubtractCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string colorCode = value == null ? "#FFFFFF" : value.ToString();
            Color c1 = (Color)ColorConverter.ConvertFromString(colorCode);
            Color c2 = Color.Subtract(Colors.White, c1);
            c2.A = 255;
            return new SolidColorBrush(c2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
