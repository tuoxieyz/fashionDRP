using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using DistributionModel;

namespace DistributionView
{
    public class VoucherItemKindIDNameCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int id = (int)value;
            var kinds = (List<VoucherItemKind>)parameter;
            var kind = kinds.Find(o => o.ID == id);
                return kind.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
