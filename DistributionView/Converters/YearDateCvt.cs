using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace DistributionView
{
    /// <summary>
    /// 年份到日期的转换
    /// <remarks>此年份为整型</remarks>
    /// </summary>
    public class YearDateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int year;
            bool flag = int.TryParse(value.ToString(), out year);
            if (!flag)
                return DateTime.Now.Date;
            if (year < 0 || year > 9999)
                return DateTime.Now.Date;
            else
            {
                return new DateTime(year, DateTime.Now.Month, DateTime.Now.Day);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;
            DateTime date = (DateTime)value;
            return date.Year;
        }
    }
}
