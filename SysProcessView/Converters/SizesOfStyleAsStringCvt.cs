using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SysProcessModel;
using System.Globalization;

namespace SysProcessView
{
    public class SizesOfStyleAsStringCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = "";
            var sizes = value as IEnumerable<ProSize>;
            if (sizes != null)
            {
                foreach (var s in sizes)
                {
                    str += (s.Name + ",");
                }
            }
            return str.TrimEnd(',');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
