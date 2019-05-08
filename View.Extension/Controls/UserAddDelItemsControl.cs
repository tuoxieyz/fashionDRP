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

namespace View.Extension
{
    public class UserAddDelItemsControl : ItemsControl
    {
        static UserAddDelItemsControl()
        {
            //假如注释掉下句，那么UserAddDelItemsControl的样式默认采用ItemsControl的样式
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserAddDelItemsControl), new FrameworkPropertyMetadata(typeof(UserAddDelItemsControl)));
        }
    }
}
