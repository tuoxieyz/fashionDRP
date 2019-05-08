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
using System.Windows.Shapes;
using ManufacturingModel;
using Telerik.Windows.Controls.GridView;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for OuterFactorySelectorWin.xaml
    /// </summary>
    public partial class OuterFactorySelectorWin : Window
    {
        public event Action<Factory> OuterFactorySelected;

        public OuterFactorySelectorWin(IEnumerable<Factory> datas)
        {
            InitializeComponent();
            RadGridView1.ItemsSource = datas;

            //同行双击
            RadGridView1.RowActivated += new EventHandler<RowEventArgs>(RadGridView1_RowActivated);
        }

        void RadGridView1_RowActivated(object sender, RowEventArgs e)
        {
            Factory o = e.Row.Item as Factory;
            if (o != null && OuterFactorySelected != null)
            {
                OuterFactorySelected(o);
                this.Close();
            }
        }
    }
}
