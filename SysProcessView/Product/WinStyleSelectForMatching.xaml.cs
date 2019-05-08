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
using SysProcessViewModel;
using Telerik.Windows.Controls;

namespace SysProcessView.Product
{
    /// <summary>
    /// Interaction logic for WinStyleSelectForMatching.xaml
    /// </summary>
    public partial class WinStyleSelectForMatching : Window
    {
        WinStyleSelectForMatchingVM _dataContxt = null;

        //internal event Action SaveSucceedEvent;

        public WinStyleSelectForMatching(StylePictureAlbum album)
        {
            _dataContxt = new WinStyleSelectForMatchingVM(album);
            this.DataContext = _dataContxt;
            InitializeComponent();
        }

        public WinStyleSelectForMatching(StylePictureAlbum album, ProStyleMatchingBO matching)
        {
            _dataContxt = new WinStyleSelectForMatchingVM(album, matching);
            this.DataContext = _dataContxt;
            InitializeComponent();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProSCPictureForMatchingBO pic = e.Parameter as ProSCPictureForMatchingBO;
            if (pic != null)
            {
                _dataContxt.SelectPicturePerStyle(pic);
                //if (_dataContxt.Entities == null || _dataContxt.Entities.Count() == 0)
                //    return;
                //var data = _dataContxt.Entities.Where(o => o.StyleCode == pic.StyleCode && o.ColorCode != pic.ColorCode);
                //foreach (var d in data)
                //{
                //    var item = lbxContainer.ItemContainerGenerator.ContainerFromItem(d) as RadListBoxItem;
                //    if (item != null)
                //        item.IsSelected = false;
                //}
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var result = _dataContxt.Save();
            MessageBox.Show(result.Message);
            if (result.IsSucceed)
            {
                //if (this.SaveSucceedEvent != null)
                //    SaveSucceedEvent();
                this.Close();
            }
        }

        //private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    Grid grid = sender as Grid;
        //    ProSCPictureForMatchingBO pic = grid.DataContext as ProSCPictureForMatchingBO;
        //    if (pic != null && !pic.IsSelected)
        //        pic.IsSelected = true;
        //}
    }
}
