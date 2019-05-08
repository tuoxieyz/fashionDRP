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

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for SubordinateRetailAchievementContrail.xaml
    /// </summary>
    public partial class SubordinateRetailAchievementContrail : UserControl
    {
        public SubordinateRetailAchievementContrail()
        {
            InitializeComponent(); 
            //billFilter.ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime) });

            //var dateFilters = new CompositeFilterDescriptor();
            //dateFilters.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date));
            //dateFilters.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date));
            //billFilter.FilterDescriptors.Add(dateFilters);

            //billFilter.ItemPropertyDefinitions.AddRange(new[] {
            //    new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)},
            //    new ItemPropertyDefinition { DisplayName = "机构类型", PropertyName = "OrganizationTypeID", PropertyType = typeof(int) }, 
            //    new ItemPropertyDefinition { DisplayName = "地区", PropertyName = "AreaID", PropertyType = typeof(int) },
            //    new ItemPropertyDefinition { DisplayName = "省份", PropertyName = "ProvienceID", PropertyType = typeof(int) }});

            //billFilter.FilterDescriptors.Add(new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue));
        }

        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    var data = ReportDataContext.GetSubordinateRetailAchievement(billFilter.FilterDescriptors);
        //    RadGridView1.ItemsSource = data;
        //}

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        //private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        //{
        //    RadComboBox cbx = e.Editor as RadComboBox;
        //    if (cbx != null)
        //    {
        //        //SysProcessView.UIHelper.SetAPTForFilter(e.ItemPropertyDefinition.PropertyName, cbx);
        //        if (e.ItemPropertyDefinition.PropertyName == "BrandID")
        //        {
        //            cbx.ItemsSource = VMGlobal.PoweredBrands;
        //        }
        //    }
        //    SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        //}
    }
}
