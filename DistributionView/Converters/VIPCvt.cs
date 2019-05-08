using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using DistributionViewModel;
using DistributionModel;
using SysProcessViewModel;
using ViewModelBasic;
using SysProcessModel;

namespace DistributionView
{
    public class VIPConsumCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                //int consum = System.Convert.ToInt32(value);
                return "满" + value.ToString();
            }
            catch { return ""; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class VIPConsumDateSpanCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int span = System.Convert.ToInt32(value);
                return "满" + span.ToString() + "天";
            }
            catch { return ""; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class VIPKindOfBrandCvt : IValueConverter
    {
        private Dictionary<int, List<VIPKind>> _brandVIPKindCache = new Dictionary<int, List<VIPKind>>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int brandID = System.Convert.ToInt32(value);
            if (!_brandVIPKindCache.ContainsKey(brandID))
            {
                var kinds = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(o => o.BrandID == brandID).ToList();
                _brandVIPKindCache.Add(brandID, kinds);
            }
            return _brandVIPKindCache[brandID];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class VIPUpTacticDecriptionCvt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string sn = (string)parameter;//是否换行
                int oneconsum = (int)values[0];
                int spandate = (int)values[1];
                int spanconsum = (int)values[2];
                string msg = "";
                if (oneconsum != 0)
                    msg = "单次消费满" + oneconsum.ToString() + "元";
                if (spandate != 0 && spanconsum != 0)
                {
                    if (!string.IsNullOrEmpty(msg))
                        msg += (sn == "1" ? "\n或\n" : "或");
                    msg += spandate.ToString() + "天内累计消费满" + spanconsum.ToString() + "元";
                }
                return msg;
            }
            catch { return ""; }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VIPKindIDNameCvt : IValueConverter
    {
        private List<VIPKind> _kinds = null;

        public VIPKindIDNameCvt()
        {
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            _kinds = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(o => brandIDs.Contains(o.BrandID)).ToList();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int id = System.Convert.ToInt32(value);
            var kind = _kinds.Find(o => o.ID == id);
            return kind == null ? "" : kind.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    internal class VIPCardKindNamesCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var kinds = value as List<VIPKind>;
            string result = "";
            if (kinds != null)
            {
                foreach (var kind in kinds)
                {
                    var brand = VMGlobal.PoweredBrands.Find(o => o.ID == kind.BrandID);
                    if (brand != null)
                    {
                        result += (brand.Name + kind.Name + ",");
                    }
                }
            }
            return result.TrimEnd(',');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    internal class VIPCardKindsCvt : IValueConverter
    {
        private List<VIPKind> _kinds = null;

        public VIPCardKindsCvt()
        {
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            _kinds = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(o => brandIDs.Contains(o.BrandID)).ToList();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int vid = System.Convert.ToInt32(value);
            var cks = VMGlobal.DistributionQuery.LinqOP.Search<VIPCardKindMapping>(o => o.CardID == vid).ToList();
            List<VIPCardKindEntity> entities = new List<VIPCardKindEntity>();
            foreach (ProBrand brand in VMGlobal.PoweredBrands)
            {
                if (_kinds.Exists(k => k.BrandID == brand.ID))//品牌是否有对应的VIP类型，若有则添加集合项
                {
                    entities.Add(new VIPCardKindEntity { BrandID = brand.ID });
                }
            }
            foreach (var entity in entities)
            {
                var kind = _kinds.Find(o => o.BrandID == entity.BrandID && cks.Exists(ck => ck.KindID == o.ID));
                if (kind != null)//vip有相应品牌的VIP等级
                {
                    entity.KindID = kind.ID;
                }
            }
            return entities;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    internal class VIPCardKindEntity
    {
        public int BrandID { get; set; }
        public int KindID { get; set; }
    }

    public class VIPKindOfBrandWithBrandNameCvt : IValueConverter
    {
        private Dictionary<int, List<IDNameImplementEntity>> _brandVIPKindCache = new Dictionary<int, List<IDNameImplementEntity>>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int brandID = System.Convert.ToInt32(value);
            if (brandID != default(int))
            {
                if (!_brandVIPKindCache.ContainsKey(brandID))
                {
                    var brand = VMGlobal.PoweredBrands.Find(o => o.ID == brandID);
                    var kinds = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(o => o.BrandID == brandID).OrderBy(o => o.ID).Select(o => new IDNameImplementEntity
                    {
                        ID = o.ID,
                        Name = brand.Name + o.Name
                    }).ToList();
                    _brandVIPKindCache.Add(brandID, kinds);
                }
                return _brandVIPKindCache[brandID];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class BirthdayAgeCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime date = System.Convert.ToDateTime(value);
            return DateTime.Now.Year - date.Year + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class VIPIDNameCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int vid = (int)value;
            return VMGlobal.DistributionQuery.LinqOP.GetById<VIPCard>(vid).CustomerName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class VIPIDCodeCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int vid = (int)value;
            return VMGlobal.DistributionQuery.LinqOP.GetById<VIPCard>(vid).Code;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
