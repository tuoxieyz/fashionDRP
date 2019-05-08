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
using Telerik.Windows.Controls.Data.DataForm;
using DistributionViewModel;
using DistributionModel;
using SysProcessViewModel;
using SysProcessView;

namespace DistributionView.VIP
{
    /// <summary>
    /// Interaction logic for VIPBirthdayTacticSet.xaml
    /// </summary>
    public partial class VIPBirthdayTacticSet : UserControl
    {
        DataFormCommandButtonsVisibility _access;
        VIPBirthdayTacticVM _dataContext = new VIPBirthdayTacticVM();

        public VIPBirthdayTacticSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            _access = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.VIP策略);
            myRadDataForm.CommandButtonsVisibility = _access;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<VIPBirthdayTactic>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VIPBirthdayTactic>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VIPBirthdayTactic kind = (VIPBirthdayTactic)myRadDataForm.CurrentItem;
            if (kind.OrganizationID != VMGlobal.CurrentUser.OrganizationID)
            {
                MessageBox.Show("只能修改本机构创建的VIP生日消费策略.");
                e.Cancel = true;
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
