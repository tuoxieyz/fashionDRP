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
using System.Windows.Controls.Primitives;

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for ButtonBar.xaml
    /// </summary>
    public partial class BottomBar : UserControl
    {
        internal event Action<Button, IWidget> WidgetButtonClick;

        public BottomBar()
        {
            InitializeComponent();
        }

        private void WidgetButton_Click(object sender, RoutedEventArgs e)
        {
            if (WidgetButtonClick != null)
            {
                Button btn = (Button)sender;
                WidgetButtonClick(btn, (IWidget)btn.DataContext);
            }
        }

        internal UIElement this[IWidget iw]
        {
            get
            {
                return (UIElement)lbxWidgetButtons.ItemContainerGenerator.ContainerFromItem(iw);
            }
        }
    }
}
