using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using VersionManager.BO;
using System.Globalization;
using VersionManager.ViewModel;

using ViewModelBasic;

namespace VersionManager
{
    internal class SoftSelectCustomerConvertor : IValueConverter, IRefresh
    {
        private CustomerListVM _customervm;
        private CustomerListVM Customervm
        {
            get
            {
                if (_customervm == null)
                {
                    _customervm = new CustomerListVM();
                }
                return _customervm;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var allcustomers = Customervm.Entities;
            //重置选择项
            foreach (var item in allcustomers)
            {
                item.IsHold = false;
            }
            SoftToUpdateBO soft = (SoftToUpdateBO)value;
            foreach (var customer in soft.Customers)
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

        public void Refresh()
        {
            Customervm.Refresh();
        }
    }
}
