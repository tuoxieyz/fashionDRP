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

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for VersionContrailWin.xaml
    /// </summary>
    public partial class VersionContrailWin : Window
    {
        public VersionContrailWin()
        {
            InitializeComponent(); 
            this.Loaded += delegate { RadBook1.Focus(); };
        }
    }
}
