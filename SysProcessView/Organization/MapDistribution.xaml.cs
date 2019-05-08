using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using Telerik.Windows.Controls.Map;

namespace SysProcessView.Organization
{
    /// <summary>
    /// MapDistribution.xaml 的交互逻辑
    /// </summary>
    public partial class MapDistribution : UserControl
    {
        public MapDistribution()
        {
            this.DataContext = new MapDistributionVM();
            InitializeComponent();
            //RadMap1.Center = new Location(36, 107);

            BingMapProvider provider = new BingMapProvider(MapMode.Road, true, "AqaPuZWytKRUA8Nm5nqvXHWGL8BDCXvK8onCl2PkC581Zp3T_fYAQBiwIphJbRAK");
            provider.IsTileCachingEnabled = true;
            RadMap1.Provider = provider;
        }

        private void RadMap1_MapMouseClick(object sender, MapMouseRoutedEventArgs eventArgs)
        {
            this.locationLayer.Items.Clear();
            this.locationLayer.Items.Add(eventArgs.Location);
        }
    }

    public class HotSpotImageCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            OrganizationShowOnMap organization = value as OrganizationShowOnMap;
            if (organization != null && organization.Latitude != null && organization.Longitude != null)
            {
                if (organization.IsOwned)
                    return new BitmapImage(new Uri("pack://application:,,,/HabilimentERP;Component/Images/smallheart.png"));
                else
                    return new BitmapImage(new Uri("pack://application:,,,/HabilimentERP;Component/Images/smallheartblue.png"));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class HotSpotLocationCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            OrganizationShowOnMap organization = value as OrganizationShowOnMap;
            //if (organization != null && organization.Latitude != null && organization.Longitude != null)
            //{
            //    return new Location(organization.Latitude.Value, organization.Longitude.Value);
            //}
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
