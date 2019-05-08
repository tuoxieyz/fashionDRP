using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace SysProcessView.Organization
{
    /// <summary>
    /// OrganizationMap.xaml 的交互逻辑
    /// </summary>
    public partial class OrganizationMap : UserControl
    {
        public OrganizationMap()
        {
            InitializeComponent();
            frameUri.Source = new Uri(ConfigurationManager.AppSettings["OrganizationMapUri"]);
        }
    }
}
