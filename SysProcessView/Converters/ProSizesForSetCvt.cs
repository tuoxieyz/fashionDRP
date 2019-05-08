using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SysProcessViewModel;
using SysProcessModel;
using System.Globalization;

namespace SysProcessView
{
    public class ProSizesForSetCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int kind = System.Convert.ToInt32(parameter);
            //ProductListVM context = parameter as ProductListVM;
            var sizes = VMGlobal.Sizes.Where(o => kind == 0 || o.Flag).Select(s => new ProSizeForSet { ID = s.ID, Code = s.Code, Name = s.Name, IsHold = false }).ToList();
            IEnumerable<ProSize> sizesHold = value as IEnumerable<ProSize>;
            if (sizesHold != null)
            {
                //var sizesHold = context.GetSizesOfStyle(id);
                foreach (var size in sizes)
                {
                    size.IsHold = false;
                    if (sizesHold.Any(s => s.ID == size.ID))
                        size.IsHold = true;
                }
            }
            return sizes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
