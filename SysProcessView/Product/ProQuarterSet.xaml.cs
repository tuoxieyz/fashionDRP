using SysProcessModel;
using SysProcessViewModel;
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

namespace SysProcessView.Product
{
    /// <summary>
    /// ProQuarterSet.xaml 的交互逻辑
    /// </summary>
    public partial class ProQuarterSet : UserControl
    {
        ProQuarterVM _dataContext = new ProQuarterVM();

        public ProQuarterSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            UIHelper.AddOrUpdateRecord<ProQuarter>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<ProQuarter>(myRadDataForm, _dataContext, e);
        }
    }
}
