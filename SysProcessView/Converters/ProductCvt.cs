using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using DomainLogicEncap;
using System.Collections;
using SysProcessModel;
using SysProcessViewModel;
using DistributionViewModel;
using System.Windows;

namespace SysProcessView
{
    public class BrandCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int id = (int)value;
            return id == 0 ? "" : VMGlobal.PoweredBrands.Find(bd => bd.ID == id).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class YearQuarterWithStyleCvt : IValueConverter
    {
        private List<ProBYQ> _byqCache = new List<ProBYQ>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProductDataContext context = parameter as ProductDataContext;
            ProStyle style = value as ProStyle;
            if (style != null && style.ID != 0)
            {
                var byq = _byqCache.Find(o => o.ID == style.BYQID);
                if (byq == null)
                {
                    byq = VMGlobal.SysProcessQuery.LinqOP.GetById<ProBYQ>(style.BYQID);
                    _byqCache.Add(byq);
                }
                return byq.Year + context.Quarters.Find(q => q.ID == byq.Quarter).Name;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class StyleColorPictureCvt : IValueConverter
    //{
    //    private string _url = "";

    //    public StyleColorPictureCvt(string url)
    //    {
    //        _url = url;
    //    }

    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var pname = value.ToString();
    //        return _url + pname;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class YearQuarterCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //为什么加异常判断？请看ProductInfo.xaml的相关注释
            try
            {
                string year = values[0].ToString();
                int quarter = (int)values[1];
                return year + VMGlobal.Quarters.Find(q => q.ID == quarter).Name;
            }
            catch { return ""; }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class StylePictureCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        ProStyleBO style = value as ProStyleBO;
    //        if (style != null && style.Colors != null && style.Colors.Count() > 0)
    //            return ProductHelper.GetProductImage(style.Code + style.Colors.First().Code);
    //        return ProductHelper.GenerateNullImage();
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class ProductCostPriceCvt : IValueConverter
    {
        private ContractDiscountHelper _helper = new ContractDiscountHelper();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                ProStyleBO style = (ProStyleBO)value;
                if (VMGlobal.CurrentUser.OrganizationID != 1)
                {
                    return style.CostPrice * _helper.GetDiscount(style.BrandID, style.Year, style.Quarter, VMGlobal.CurrentUser.OrganizationID) * 0.01M;
                }
                return style.CostPrice;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ProductCostPriceVisibleCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return VMGlobal.CurrentUser.OrganizationID == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
