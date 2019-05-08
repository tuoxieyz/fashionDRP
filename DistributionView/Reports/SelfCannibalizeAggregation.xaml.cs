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
using DomainLogicEncap;
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using SysProcessViewModel;
using SysProcessView;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SelfCannibalizeAggregation.xaml
    /// </summary>
    public partial class SelfCannibalizeAggregation : UserControl
    {
        SelfCannibalizeAggregationVM _dataContext = new SelfCannibalizeAggregationVM();

        public SelfCannibalizeAggregation()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            SysProcessView.UIHelper.TransferSizeToHorizontal(RadGridView1);
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
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
            else
            {
                OrganizationSelector os = e.Editor as OrganizationSelector;
                if (os != null)
                {
                    switch (e.ItemPropertyDefinition.PropertyName)
                    {
                        case "OrganizationID":
                        case "ToOrganizationID":
                            os.ItemsSource = OrganizationLogic.GetSiblingOrganizations(VMGlobal.CurrentUser.OrganizationID);
                            break;
                    }
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
