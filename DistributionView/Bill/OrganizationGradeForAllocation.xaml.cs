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
using ERPViewModelBasic;
using DistributionViewModel;
using View.Extension;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrganizationGradeForAllocation.xaml
    /// </summary>
    public partial class OrganizationGradeForAllocation : UserControl
    {
        OrganizationGradeForAllocationVM _dataContext = new OrganizationGradeForAllocationVM();

        public OrganizationGradeForAllocation()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
            }
        }

        private void myRadDataForm_EditEnding(object sender, Telerik.Windows.Controls.Data.DataForm.EditEndingEventArgs e)
        {
            UIHelper.AddOrUpdateRecord<OrganizationAllocationGrade>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UIHelper.DeleteRecord<OrganizationAllocationGrade>(myRadDataForm, _dataContext, e);
        }

        private void btnBatchSet_Click(object sender, RoutedEventArgs e)
        {
            OrganizationAllocationGradeBatchSetWin win = new OrganizationAllocationGradeBatchSetWin();
            win.Owner = UIHelper.GetAncestor<Window>(this);
            win.SetCompleted += delegate
            {
                _dataContext.SearchCommand.Execute(null);
            };
            win.ShowDialog();
        }
    }
}
