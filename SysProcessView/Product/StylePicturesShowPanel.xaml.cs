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
using SysProcessViewModel;
using Telerik.Windows.Controls;
using SysProcessView.Product;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for StylePicturesShowPanel.xaml
    /// </summary>
    public partial class StylePicturesShowPanel : UserControl
    {
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(StylePicturesShowPanel), new PropertyMetadata(false, OnIsReadOnlyChanged));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        //private bool _readOnly = false;
        //public bool ReadOnly
        //{
        //    private get { return _readOnly; }
        //    set
        //    {
        //        _readOnly = value;
        //        if (value)
        //            btnAddMatching.Visibility = System.Windows.Visibility.Collapsed;
        //        else
        //            btnAddMatching.Visibility = System.Windows.Visibility.Visible;
        //    }
        //}

        public StylePicturesShowPanel()
        {
            InitializeComponent();
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isReadOnly = (bool)e.NewValue;
            StylePicturesShowPanel panel = d as StylePicturesShowPanel;
            if (isReadOnly)
                panel.btnAddMatching.Visibility = System.Windows.Visibility.Collapsed;
            else
                panel.btnAddMatching.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnAddMatching_Click(object sender, RoutedEventArgs e)
        {
            StylePictureAlbum album = this.DataContext as StylePictureAlbum;
            if (album != null)
            {
                WinStyleSelectForMatching win = new WinStyleSelectForMatching(album);
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                //win.SaveSucceedEvent += () =>
                //{
                //    style.SelectedPicture.Matchings = null;
                //    BindingExpression be = icMatching.GetBindingExpression(ItemsControl.ItemsSourceProperty);
                //    be.UpdateTarget();
                //};
                win.ShowDialog();
            }
        }

        private void matchingOperationStateChange(object sender, MouseEventArgs e)
        {
            if (!IsReadOnly)
            {
                Grid grid = sender as Grid;
                if (grid.IsMouseOver)
                {
                    VisualStateManager.GoToElementState(grid, "MouseOver", true);
                }
                else
                {
                    VisualStateManager.GoToElementState(grid, "Normal", true);
                }
            }
        }

        private void btnDeleteMatching_Click(object sender, RoutedEventArgs e)
        {
            var diaResult = MessageBox.Show("确定要删除该搭配组吗?", "提醒", MessageBoxButton.OKCancel);
            if (diaResult == MessageBoxResult.OK)
            {
                RadButton btn = sender as RadButton;
                ProStyleMatchingBO matching = btn.DataContext as ProStyleMatchingBO;
                if (matching != null)
                {
                    var result = ProStylePictureBookVM.DeleteMatchingGroup(matching.GroupID);
                    MessageBox.Show(result.Message);
                    if (result.IsSucceed)
                    {
                        StylePictureAlbum album = this.DataContext as StylePictureAlbum;
                        if (album != null)
                        {
                            //album.SelectedStyle.SelectedPicture.Matchings.Remove(matching);
                            ProStylePictureBookVM.DeleteMatchingForAlbum(album, matching);
                        }
                    }
                }
            }
        }

        private void btnEditMatching_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            ProStyleMatchingBO matching = btn.DataContext as ProStyleMatchingBO;
            if (matching != null)
            {
                StylePictureAlbum album = this.DataContext as StylePictureAlbum;
                if (album != null)
                {
                    WinStyleSelectForMatching win = new WinStyleSelectForMatching(album, matching);
                    win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                    win.ShowDialog();
                }
            }
        }
    }

    public class ColorIDNameCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int id = (int)value;
            return VMGlobal.Colors.Find(o => o.ID == id).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
