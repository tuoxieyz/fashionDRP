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
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls;

namespace View.Extension
{
    /// <summary>
    /// Interaction logic for SelectWithGridViewWin.xaml
    /// </summary>
    public partial class SelectWithGridViewWin : Window
    {
        public event Action<IEnumerable<object>> SelectionCompleted;

        public static readonly DependencyProperty IsMultiSelectableProperty =
        DependencyProperty.Register("IsMultiSelectable", typeof(bool), typeof(SelectWithGridViewWin), new PropertyMetadata(false, new PropertyChangedCallback(OnIsMultiSelectableChanged)));

        public bool IsMultiSelectable
        {
            get { return (bool)GetValue(IsMultiSelectableProperty); }
            set { SetValue(IsMultiSelectableProperty, value); }
        }

        public SelectWithGridViewWin()
        {
            InitializeComponent();
            RadGridView1.RowActivated += new EventHandler<RowEventArgs>(RadGridView1_RowActivated);
        }

        void RadGridView1_RowActivated(object sender, RowEventArgs e)
        {
            if (SelectionCompleted != null)
            {
                SelectionCompleted(new object[] { e.Row.Item });
            }
            this.Close();
        }

        private static void OnIsMultiSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (SelectWithGridViewWin)d;
            bool isMultiSelectable = (bool)e.NewValue;
            win.RadGridView1.Columns[0].IsVisible = isMultiSelectable;
            win.btnOK.Visibility = isMultiSelectable ? Visibility.Visible : Visibility.Collapsed;
            win.RadGridView1.SelectionMode = isMultiSelectable ? SelectionMode.Extended : SelectionMode.Single;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SelectionCompleted != null)
            {
                SelectionCompleted(this.RadGridView1.SelectedItems);
            }
            this.Close();
        }
    }
}
