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
using Telerik.Windows.Controls;
using Telerik.Windows;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrderWithStyleAlbum.xaml
    /// </summary>
    public partial class OrderWithStyleAlbum : UserControl
    {
        public OrderWithStyleAlbum()
        {
            this.DataContext = new ProStylePictureBookVM();   
            InitializeComponent();
        }

        private void tileView1_TileStateChanged(object sender, RadRoutedEventArgs e)
        {
            RadTileViewItem item = e.OriginalSource as RadTileViewItem;
            if (item != null)
            {
                RadFluidContentControl fluid = item.ChildrenOfType<RadFluidContentControl>().FirstOrDefault();
                if (fluid != null)
                {
                    switch (item.TileState)
                    {
                        case TileViewItemState.Maximized:
                            fluid.State = FluidContentControlState.Large;
                            break;
                        case TileViewItemState.Minimized:
                            fluid.State = FluidContentControlState.Normal;
                            break;
                        case TileViewItemState.Restored:
                            fluid.State = FluidContentControlState.Normal;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
