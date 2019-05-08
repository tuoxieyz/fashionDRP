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
using DistributionModel;
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessView;
using SysProcessViewModel;

namespace DistributionView.VIP
{
    /// <summary>
    /// Interaction logic for VIPUpTacticSet.xaml
    /// </summary>
    public partial class VIPUpTacticSet : UserControl
    {
        DataFormCommandButtonsVisibility _access;
        VIPUpTacticVM _dataContext = new VIPUpTacticVM();

        public VIPUpTacticSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            _access = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.VIP策略);
            myRadDataForm.CommandButtonsVisibility = _access;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<VIPUpTactic>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VIPUpTactic>(myRadDataForm, _dataContext, e);
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            myRadDataForm.ValidationSummary.Errors.Clear();
        }
    }
}
