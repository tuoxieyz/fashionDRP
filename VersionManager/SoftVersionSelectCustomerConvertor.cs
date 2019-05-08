using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using VersionManager.BO;

namespace VersionManager
{
    internal class SoftVersionSelectCustomerConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SoftVersionTrackBO version = (SoftVersionTrackBO)value;
            var allcustomers = version.Soft.Customers;
            foreach (var customer in version.Customers)
            {
                var item = allcustomers.FirstOrDefault(o => o.ID == customer.ID);
                if (item != null)
                {
                    item.IsHold = true;
                }
            }
            return allcustomers;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
