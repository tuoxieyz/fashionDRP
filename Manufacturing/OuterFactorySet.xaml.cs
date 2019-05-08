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
using ManufacturingModel;
using Manufacturing.ViewModel;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for OuterFactorySet.xaml
    /// </summary>
    public partial class OuterFactorySet : UserControl
    {
        OuterFactoryVM _dataContext = new OuterFactoryVM();

        public OuterFactorySet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<Factory>(myRadDataForm, _dataContext, e);
            if (e.Cancel == false)
            {
                RadGridView1.Rebind();
            }
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<Factory>(myRadDataForm, _dataContext, e);
        }
    }
}
