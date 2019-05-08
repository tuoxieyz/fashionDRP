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
using SysProcessModel;
using Telerik.Windows.Controls.Data.DataForm;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for ProBrandSet.xaml
    /// </summary>
    public partial class ProBrandSet : UserControl
    {
        ProBrandVM _dataContext = new ProBrandVM();

        public ProBrandSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            UIHelper.AddOrUpdateRecord<ProBrand>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("品牌一旦创建就不能删除，\n若不使用，请将状态置为禁用。");
            e.Cancel = true;
        }
    }
}
