using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace View.Extension
{
    /// <summary>
    /// 数据相加转换
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class NumericPlusCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double offset = System.Convert.ToDouble(parameter);
            return (double)value + offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class NumericMultiplicationCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double offset = System.Convert.ToDouble(parameter);
            return (double)value * offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
