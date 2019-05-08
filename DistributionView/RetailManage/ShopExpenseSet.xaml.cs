using DistributionModel;
using DistributionViewModel;
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
using Telerik.Windows.Controls.Data.DataForm;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// ShopExpenseSet.xaml 的交互逻辑
    /// </summary>
    public partial class ShopExpenseSet : UserControl
    {
        ShopExpenseSetVM _dataContext = new ShopExpenseSetVM();

        public ShopExpenseSet()
        {
            this.DataContext = _dataContext;

            this.Resources.Add("expenseKinds", _dataContext.ExpenseKinds);
            var enabledList = _dataContext.ExpenseKinds.Where(o => o.IsEnabled);
            this.Resources.Add("enabledExpenseKinds", enabledList);

            InitializeComponent();
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                if (e.ItemPropertyDefinition.PropertyName == "ExpenseKindID")
                {
                    cbx.ItemsSource = _dataContext.ExpenseKinds;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<ShopExpense>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<ShopExpense>(myRadDataForm, _dataContext, e);
        }
        

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
