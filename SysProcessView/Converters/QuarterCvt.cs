using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SysProcessViewModel;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace SysProcessView
{
    public class QuarterCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int quarter = (int)value;
            return quarter == 0 ? "" : VMGlobal.Quarters.Find(q => q.ID == quarter).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuarterImageCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int quarter = (int)value;
            quarter = quarter % 4; 
            switch (quarter)
            {
                case 1: return new BitmapImage(new Uri("pack://application:,,,/HabilimentERP;component/Images/chun.png"));
                case 2: return new BitmapImage(new Uri("pack://application:,,,/HabilimentERP;component/Images/xia.png"));
                case 3: return new BitmapImage(new Uri("pack://application:,,,/HabilimentERP;component/Images/qiu.png"));
                case 0: return new BitmapImage(new Uri("pack://application:,,,/HabilimentERP;component/Images/dong.png"));
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
