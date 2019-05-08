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
using DomainLogicEncap;
using SysProcessViewModel;
using SysProcessView;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for OtherShopStockStatistics.xaml
    /// </summary>
    public partial class OtherShopStockStatistics : UserControl
    {
        public OtherShopStockStatistics()
        {
            InitializeComponent();
            //SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
            ItemPropertyDefinition[] billConditions = new[] {
                new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) }};

            billFilter.ItemPropertyDefinitions.AddRange(billConditions);

            billFilter.FilterDescriptors.Add(new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RadGridView1.ItemsSource = ReportDataContext.GetOtherShopStock(billFilter.FilterDescriptors);
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "BrandID":
                        cbx.ItemsSource = VMGlobal.PoweredBrands;
                        break;
                }
            }
            else
            {
                OrganizationSelector os = e.Editor as OrganizationSelector;
                if (os != null)
                {
                    switch (e.ItemPropertyDefinition.PropertyName)
                    {
                        case "OrganizationID":
                            os.ItemsSource = OrganizationLogic.GetSiblingShops(VMGlobal.CurrentUser.OrganizationID);
                            break;
                    }
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }
    }
}
