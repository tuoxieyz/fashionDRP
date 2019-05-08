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
using SysProcessViewModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessModel;

namespace SysProcessView.Organization
{
    /// <summary>
    /// Interaction logic for OrganizationTypeSet.xaml
    /// </summary>
    public partial class OrganizationTypeSet : UserControl
    {
        OrganizationTypeVM _dataContext = new OrganizationTypeVM();

        public OrganizationTypeSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            UIHelper.AddOrUpdateRecord<SysOrganizationType>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<SysOrganizationType>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            SysOrganizationType type = (SysOrganizationType)myRadDataForm.CurrentItem;
            type.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }
}
