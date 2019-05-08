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
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;
using View.Extension;
using SysProcessViewModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for MonthSaleTaget.xaml
    /// </summary>
    public partial class MonthSaleTaget : UserControl
    {
        MonthSaleTagetVM _dataContext = new MonthSaleTagetVM();

        public MonthSaleTaget()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<RetailMonthTaget>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<RetailMonthTaget>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RetailMonthTaget kind = (RetailMonthTaget)myRadDataForm.CurrentItem;
            if (kind.OrganizationID == VMGlobal.CurrentUser.OrganizationID)
            {
                MessageBox.Show("不能修改本机构自身的月度指标.");
                e.Cancel = true;
            }
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "Year")
            {
                RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
                //dateTimePickerEditor.InputMode = Telerik.Windows.Controls.InputMode.DatePicker;
                dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                {
                    if (ee.AddedItems.Count > 0)
                    {
                        DateTime date = (DateTime)ee.AddedItems[0];
                        dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    }
                };
            }
            //bug,虽然value变了，但显示在界面上的还是1月，估计是作为查询条件控件时就有的bug
            //else if (e.ItemPropertyDefinition.PropertyName == "Month")
            //{
            //    RadNumericUpDown num = (RadNumericUpDown)e.Editor;
            //    num.Value = DateTime.Now.Month;
            //}
        }

        private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RadDatePicker datePicker = (RadDatePicker)sender;
                DateTime date = (DateTime)e.AddedItems[0];
                datePicker.DateTimeText = date.ToString("yyyy-MM");
            }
        }

        private void btnBatchSet_Click(object sender, RoutedEventArgs e)
        {
            MonthSaleTargetBatchSetWin win = new MonthSaleTargetBatchSetWin();
            win.Owner = UIHelper.GetAncestor<Window>(this);
            win.SetCompleted += delegate
            {
                _dataContext.SearchCommand.Execute(null);
            };
            win.ShowDialog();
        }
    }
}
