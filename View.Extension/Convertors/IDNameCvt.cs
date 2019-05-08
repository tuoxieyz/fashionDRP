using Model.Extension;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace View.Extension
{
    public class IDNameCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int id = (int)value;
            var entities = (IEnumerable<IDNameEntity>)parameter;
            var kind = entities.FirstOrDefault(o => o.ID == id);
            if (kind != null)
                return kind.Name;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
