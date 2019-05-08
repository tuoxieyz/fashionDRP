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
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using SysProcessViewModel;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessModel;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for ColorDefine.xaml
    /// </summary>
    public partial class ColorDefine : UserControl
    {
        ProColorVM _dataContext = new ProColorVM();

        public ColorDefine()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            UIHelper.AddOrUpdateRecord<ProColor>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<ProColor>(myRadDataForm, _dataContext, e);
        }
    }
}
