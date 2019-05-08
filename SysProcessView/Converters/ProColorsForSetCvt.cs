using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using SysProcessViewModel;
using SysProcessModel;

namespace SysProcessView
{
    public class ProColorsForSetCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProductListVM context = parameter as ProductListVM;
            //请注意这里为什么要加ToList方法，参考项目手记第69条
            var colors = VMGlobal.Colors.Select(c => new ProColorForSet { ID = c.ID, Code = c.Code, Name = c.Name, RGBCode = c.RGBCode, IsHold = false }).ToList();
            IEnumerable<ProColor> colorsHold = value as IEnumerable<ProColor>;
            if (colorsHold != null)
            {
                //var colorsHold = context.GetColorsOfStyle(id);
                foreach (var color in colors)
                {
                    color.IsHold = false;
                    if (colorsHold.Any(c => c.ID == color.ID))
                        color.IsHold = true;
                }
            }
            return colors;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
