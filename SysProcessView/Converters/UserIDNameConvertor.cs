using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SysProcessModel;
using System.Globalization;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace SysProcessView
{
    public class UserIDNameConvertor : IValueConverter
    {
        private List<SysUser> _userIDNameCache;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_userIDNameCache == null)
                _userIDNameCache = new List<SysUser>();
            int userID = (int)value;
            var user = _userIDNameCache.Find(o => o.ID == userID);
            if (user == null)
            {
                user = VMGlobal.SysProcessQuery.LinqOP.GetById<SysUser>(userID);
                user = user ?? new SysUser();
                _userIDNameCache.Add(user);
            }
            return user.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
