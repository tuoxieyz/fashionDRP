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
using SysProcessModel;
using Telerik.Windows.Controls.GridView;

namespace SysProcessView.Certification
{
    /// <summary>
    /// Interaction logic for FloatPriceTacticSelectWin.xaml
    /// </summary>
    public partial class FloatPriceTacticSelectWin : Window
    {
        public event Action<OrganizationPriceFloat> SelectionCompleted;

        public FloatPriceTacticSelectWin()
        {
            InitializeComponent();
            RadGridView1.RowActivated += new EventHandler<RowEventArgs>(RadGridView1_RowActivated);
        }

        void RadGridView1_RowActivated(object sender, RowEventArgs e)
        {
            OrganizationPriceFloat o = e.Row.Item as OrganizationPriceFloat;
            if (o != null && SelectionCompleted != null)
            {
                SelectionCompleted(o);
                this.Close();
            }
        }
    }
}
