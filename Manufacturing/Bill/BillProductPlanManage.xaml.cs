using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace Manufacturing.Bill
{
    /// <summary>
    /// Interaction logic for BillProductPlanManage.xaml
    /// </summary>
    public partial class BillProductPlanManage : UserControl
    {
        public BillProductPlanManage()
        {
            InitializeComponent();
        }
    }

    public class ButtonVisibleWithPlanStateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string statusName = value.ToString();//单据状态
            return isDeletedName == "已作废" ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
