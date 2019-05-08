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
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for RetailShoppingGuideSet.xaml
    /// </summary>
    public partial class RetailShoppingGuideSet : UserControl
    {
        RetailShoppingGuideVM _dataContext = new RetailShoppingGuideVM();

        public RetailShoppingGuideSet()
        {
            this.DataContext = _dataContext;
            var shifts = VMGlobal.DistributionQuery.LinqOP.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            this.Resources.Add("resShifts", shifts);
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<RetailShoppingGuide>(myRadDataForm, _dataContext, e);
            if (!e.Cancel)//myRadDataForm.Mode== RadDataFormMode.AddNew && 
                RadGridView1.Rebind();
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<RetailShoppingGuide>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            RetailShoppingGuide guide = (RetailShoppingGuide)myRadDataForm.CurrentItem;
            guide.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }
}
