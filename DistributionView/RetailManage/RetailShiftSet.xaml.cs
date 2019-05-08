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

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for RetailShiftSet.xaml
    /// </summary>
    public partial class RetailShiftSet : UserControl
    {
        RetailShiftVM _dataContext = new RetailShiftVM();

        public RetailShiftSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<RetailShift>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<RetailShift>(myRadDataForm, _dataContext, e);
        }

        //private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    RetailShift shift = (RetailShift)myRadDataForm.CurrentItem;
        //    if (shift.OrganizationID != VMGlobal.CurrentUser.OrganizationID)
        //    {
        //        MessageBox.Show("只能修改本机构创建的班次信息.");
        //        e.Cancel = true;
        //    }
        //}

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            RetailShift shift = (RetailShift)myRadDataForm.CurrentItem;
            shift.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }
}
