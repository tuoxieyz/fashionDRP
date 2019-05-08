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
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using DistributionViewModel;
using SysProcessViewModel;
using SysProcessView;
using View.Extension;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SubordinateRetailAggregation.xaml
    /// </summary>
    public partial class SubordinateRetailAggregation : UserControl
    {
        //private ProductPictrueShowWin _showPictrueWin;

        public SubordinateRetailAggregation()
        {
            this.DataContext = new SubordinateRetailAggregationVM();
            InitializeComponent();
            var pan = new Pan();
            pan.Invest(bdPictrueShow, bdPictrueShow);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "Year":
                    RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
                    dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                    {
                        DateTime date = (DateTime)ee.AddedItems[0];
                        dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    };
                    break;
                case "Quarter":
                    RadComboBox cbxQuarter = (RadComboBox)e.Editor;
                    cbxQuarter.ItemsSource = VMGlobal.Quarters;
                    break;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void RadGridView1_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            //if (e.AddedItems == null || e.AddedItems.Count != 1)
            //{
            //    if (_showPictrueWin != null)
            //        _showPictrueWin.Close();
            //}
            //else
            //{
            //    RetailAggregationEntity entity = (RetailAggregationEntity)e.AddedItems[0];
            //    if (_showPictrueWin == null)
            //    {
            //        _showPictrueWin = new ProductPictrueShowWin();
            //        var parentWin = View.Extension.UIHelper.GetAncestor<Window>(this);
            //        _showPictrueWin.Owner = parentWin;
            //        _showPictrueWin.Loaded += delegate
            //        {
            //            _showPictrueWin.Left = parentWin.ActualWidth - _showPictrueWin.ActualWidth - 5;
            //            _showPictrueWin.Top = parentWin.ActualHeight - _showPictrueWin.ActualHeight - 5;
            //        };
            //    }
            //    _showPictrueWin.DataContext = entity.Picture;
            //    _showPictrueWin.Show();
            //}
            if (e.AddedItems == null || e.AddedItems.Count != 1)
            {
                pnlPictrueShow.Content = null;
                bdPictrueShow.Visibility = Visibility.Collapsed;
            }
            else
            {
                RetailAggregationEntity entity = (RetailAggregationEntity)e.AddedItems[0];
                pnlPictrueShow.Content = ProductHelper.GetProductImage(entity.StyleID, entity.ColorID);//entity.Picture;
                if (bdPictrueShow.Visibility == Visibility.Collapsed)
                {
                    bdPictrueShow.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
