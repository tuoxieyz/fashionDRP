using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using DistributionViewModel;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace DistributionView
{
    #region 订单相关

    public class OrderStateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isValid = (bool)value;
            return isValid ? "有效" : "已作废";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RevertButtonVisibleWithOrderStateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDeleted = !(bool)value;
            return isDeleted ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ButtonVisibleWithOrderStateCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isDeleted = !(bool)values[0];//订单状态
                string state = values[1].ToString();//发货状态
                string btnName = parameter.ToString();//按钮名称
                switch (btnName)
                {
                    case "整单作废":
                        if (!isDeleted && state == "未发货")
                            return Visibility.Visible;
                        break;
                    case "取消未完成数量":
                        if (!isDeleted && state == "部分已发货")
                            return Visibility.Visible;
                        break;
                }
                return Visibility.Hidden;
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ZeroButtonVisibleWithOrderStateCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isDeleted = !(bool)values[0];//订单状态
                int cancelQua = (int)values[1];
                if (!isDeleted && cancelQua > 0)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
            catch
            {
                return Visibility.Hidden;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 订单最大可以取消量=订单量-已发货量
    /// </summary>
    public class OrderMaxCancelQuantityCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int orderQua = (int)values[0];
                int deliverQua = (int)values[1];
                return orderQua - deliverQua;
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ButtonVisibleWithStocktakeStateCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool isDeleted = (bool)values[0];
                bool status = (bool)values[1];
                if (!isDeleted && !status)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    public class ABCAnalysisBarColorCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ABCEntity entity = (ABCEntity)value;
            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
