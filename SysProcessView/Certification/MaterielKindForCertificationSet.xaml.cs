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
using SysProcessViewModel;
using SysProcessModel;

namespace SysProcessView.Certification
{
    /// <summary>
    /// Interaction logic for MaterielKindForCertificationSet.xaml
    /// </summary>
    public partial class MaterielKindForCertificationSet : UserControl
    {
        MaterielKindForCertificationSetVM _dataContext = new MaterielKindForCertificationSetVM();

        public MaterielKindForCertificationSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            SysProcessView.UIHelper.AddOrUpdateRecord<CertMaterielKind>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<CertMaterielKind>(myRadDataForm, _dataContext, e);
        }
    }
}
