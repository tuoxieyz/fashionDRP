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
using DistributionModel;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessViewModel;
using SysProcessView;

namespace DistributionView.VIP
{
    /// <summary>
    /// Interaction logic for VIPKindSet.xaml
    /// </summary>
    public partial class VIPKindSet : UserControl
    {
        DataFormCommandButtonsVisibility _access;
        VIPKindVM _dataContext = new VIPKindVM();

        public VIPKindSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            _access = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.VIP类型);
            myRadDataForm.CommandButtonsVisibility = _access;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<VIPKind>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VIPKind>(myRadDataForm, _dataContext, e);
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
