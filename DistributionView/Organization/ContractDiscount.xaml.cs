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
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;
using DistributionViewModel;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataForm;
using DomainLogicEncap;
using SysProcessViewModel;


namespace DistributionView.Organization
{
    /// <summary>
    /// Interaction logic for ContractDiscount.xaml
    /// </summary>
    public partial class ContractDiscount : UserControl
    {
        ContractDiscountVM _dataContext = new ContractDiscountVM();

        public ContractDiscount()
        {
            this.DataContext = _dataContext;

            InitializeComponent();
        }

        private void radDataFilter_EditorCreated(object sender, EditorCreatedEventArgs e)
        {
            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "Year":
                    //RadDateTimePicker dateTimePickerEditor = (RadDateTimePicker)e.Editor;
                    //dateTimePickerEditor.DateSelectionMode = DateSelectionMode.Year;
                    //dateTimePickerEditor.IsTooltipEnabled = true;
                    //dateTimePickerEditor.ErrorTooltipContent = "输入格式不正确";
                    //dateTimePickerEditor.DateTimeWatermarkContent = "选择年份";
                    //dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                    //{
                    //    DateTime date = (DateTime)ee.AddedItems[0];
                    //    dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    //};
                    //break;
                    RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
                    //dateTimePickerEditor.InputMode = Telerik.Windows.Controls.InputMode.DatePicker;
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
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<OrganizationContractDiscount>(myRadDataForm, _dataContext, e);
            if (!e.Cancel)
                RadGridView1.Rebind();
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<OrganizationContractDiscount>(myRadDataForm, _dataContext, e);            
        }

        private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DateTime date = (DateTime)e.AddedItems[0];
                RadDatePicker picker = sender as RadDatePicker;
                picker.DateTimeText = date.Year.ToString();
            }
        }

        private void btnBatchSet_Click(object sender, RoutedEventArgs e)
        {
            ContractDiscountBatchSetWin win = new ContractDiscountBatchSetWin();
            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.SetCompleted += delegate
            {
                _dataContext.SearchCommand.Execute(null);
            };
            win.ShowDialog();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
