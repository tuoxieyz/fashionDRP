﻿using System;
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
using DomainLogicEncap;
using DistributionViewModel;
using DistributionModel;
using Telerik.Windows.Controls.GridView;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillStocktakeContrastSearch.xaml
    /// </summary>
    public partial class BillStocktakeContrastSearch : UserControl
    {
        //private FloatPriceHelper _fpHelper;

        public BillStocktakeContrastSearch()
        {
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "StorageID":
                        cbx.ItemsSource = StorageInfoVM.Storages;
                        break;
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var gv = (RadGridView)e.DetailsElement;
                var item = (ContrastSearchEntity)e.Row.Item;
                var details = ReportDataContext.GetBillStocktakeContrastDetails(item.ID);
                //if (_fpHelper == null)
                //    _fpHelper = new FloatPriceHelper();
                //foreach (var d in details)
                //{
                //    d.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, d.ProductID);
                //}
                gv.ItemsSource = details;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            var item = (ContrastSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.BillExportExcel("盈亏单", RadGridView1, item);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var item = (ContrastSearchEntity)((RadButton)sender).DataContext;
            SysProcessView.UIHelper.PrintBill("盈亏单", RadGridView1, item);
        }
    }
}
