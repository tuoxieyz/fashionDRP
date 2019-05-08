using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using DistributionModel;
using System.Windows;
using DistributionViewModel;
using SysProcessViewModel;

namespace DistributionView
{
    public class GuideWorkStateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RetailShoppingGuide guide = value as RetailShoppingGuide;
            if (guide != null)
            {
                bool flag = guide.DimissionDate != null && guide.DimissionDate.Value <= DateTime.Now;
                flag |= (guide.OnBoardDate > DateTime.Now);
                return flag ? "离职" : "在职";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RetailShiftIDNameCvt : IValueConverter
    {
        private List<RetailShift> _shifts = VMGlobal.DistributionQuery.LinqOP.Search<RetailShift>().ToList();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int id = (int)value;
            var shift = _shifts.Find(o => o.ID == id);
            return (shift == null) ? "" : shift.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RetailGuidesWithShiftCvt : IValueConverter
    {
        private List<RetailShoppingGuide> _guides = VMGlobal.DistributionQuery.LinqOP.Search<RetailShoppingGuide>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.State && o.OnBoardDate <= DateTime.Now && (o.DimissionDate == null || o.DimissionDate > DateTime.Now.Date)).ToList();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? sid = (int?)value;
            if (sid == null)
                return _guides;
            return _guides.FindAll(o => o.ShiftID == sid);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RetailTacticVisibilityCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int kind = System.Convert.ToInt32(value);
            int flag = System.Convert.ToInt32(parameter);
            if (kind == 3 || kind == flag || kind == 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RetailTacticKindCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int kind = System.Convert.ToInt32(value);
            return Enum.GetName(typeof(RetailTacticKind), kind);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VIPPredepositCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? vid = (Nullable<int>)value;
            if (vid == null)
                return 0;
            else
                return VMGlobal.DistributionQuery.LinqOP.Search<VIPPredepositTrack>(o => o.VIPID == vid).Sum(o => o.StoreMoney + o.FreeMoney - o.ConsumeMoney);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
