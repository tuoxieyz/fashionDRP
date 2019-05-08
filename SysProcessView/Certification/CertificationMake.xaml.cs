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
using System.Globalization;
using SysProcessView.Certification;
using DomainLogicEncap;
using Telerik.Windows.Controls;
using Kernel;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls.Data.DataForm;
using View.Extension;
using SysProcessModel;
using SysProcessViewModel;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for CertificationMake.xaml
    /// </summary>
    public partial class CertificationMake : UserControl
    {
        CertificationMakeVM _dataContext = new CertificationMakeVM();
        FloatPriceHelper _fpHelper;

        public CertificationMake()
        {
            this.DataContext = _dataContext;
            this.Resources.Add("context", _dataContext);
            InitializeComponent();
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "Grade":
                        cbx.ItemsSource = _dataContext.AvailableGrades;
                        break;
                    case "CarriedStandard":
                        cbx.ItemsSource = _dataContext.AvailableCarriedStandards;
                        break;
                    case "SafetyTechnique":
                        cbx.ItemsSource = _dataContext.AvailableSafetyTechs;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void PART_AddButton_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var groupbox = btn.ParentOfType<System.Windows.Controls.GroupBox>();
            var materielInfos = groupbox.DataContext as ObservableCollection<MaterielInfo>;
            var lbxMateriels = View.Extension.UIHelper.GetVisualChild<ListBox>(groupbox, "lbxMateriels");
            if (lbxMateriels.SelectedIndex != -1)
            {
                materielInfos.Insert(lbxMateriels.SelectedIndex + 1, new MaterielInfo());
            }
            else
                materielInfos.Add(new MaterielInfo());
        }

        private void PART_DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var groupbox = btn.ParentOfType<System.Windows.Controls.GroupBox>();
            var lbxMateriels = View.Extension.UIHelper.GetVisualChild<ListBox>(groupbox, "lbxMateriels");
            if (lbxMateriels.SelectedItem != null)
            {
                var materielInfos = groupbox.DataContext as ObservableCollection<MaterielInfo>;
                materielInfos.Remove((MaterielInfo)lbxMateriels.SelectedItem);
            }
        }

        private void btnAddPercent_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var groupbox = btn.ParentOfType<System.Windows.Controls.GroupBox>();
            var lbxMaterielPercents = View.Extension.UIHelper.GetVisualChild<ListBox>(groupbox, "lbxMaterielPercents");
            MaterielInfo info = btn.DataContext as MaterielInfo;
            if (lbxMaterielPercents.SelectedIndex != -1)
            {
                info.MaterielPercents.Insert(lbxMaterielPercents.SelectedIndex + 1, new MaterielPercent());
            }
            else
                info.MaterielPercents.Add(new MaterielPercent());
        }

        private void btnDeletePercent_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var groupbox = btn.ParentOfType<System.Windows.Controls.GroupBox>();
            var lbxMaterielPercents = View.Extension.UIHelper.GetVisualChild<ListBox>(groupbox, "lbxMaterielPercents");
            if (lbxMaterielPercents.SelectedItem != null)
            {
                MaterielInfo info = btn.DataContext as MaterielInfo;
                info.MaterielPercents.Remove((MaterielPercent)lbxMaterielPercents.SelectedItem);
            }
        }

        private void myRadDataForm_EditEnding(object sender, Telerik.Windows.Controls.Data.DataForm.EditEndingEventArgs e)
        {
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                SysProcessModel.Certification entity = (SysProcessModel.Certification)myRadDataForm.CurrentItem;
                var lbxMateriels = View.Extension.UIHelper.GetDataFormField<ListBox>(myRadDataForm, "lbxMateriels");
                entity.Composition = SerializeHelper.XmlObject(lbxMateriels.ItemsSource);//序列化
                SysProcessView.UIHelper.AddOrUpdateRecord<SysProcessModel.Certification>(myRadDataForm, _dataContext, e);
                if (!e.Cancel)
                {
                    RadGridView1.Rebind();
                }
            }
        }

        private void cbxMaterielPercent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RadComboBox cbx = (RadComboBox)sender;
                if (!string.IsNullOrEmpty(cbx.Text))
                {
                    IEnumerable<CertMateriel> materiels = cbx.ItemsSource as IEnumerable<CertMateriel>;
                    if (materiels != null)
                    {
                        var materiel = materiels.FirstOrDefault(o => o.Code == cbx.Text && o.Enabled);
                        if (materiel != null)
                        {
                            cbx.SelectedItem = materiel;
                        }
                    }
                }
            }
        }

        private void btnFloatPrice_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var entity = btn.DataContext as CertificationBO;
            if (entity.StyleID != default(int))
            {
                FloatPriceTacticSelectWin win = new FloatPriceTacticSelectWin();
                win.DataContext = _dataContext.GetFloatPriceTactics(entity.Style.Code);
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                win.SelectionCompleted += delegate(OrganizationPriceFloat pf)
                {
                    if (_fpHelper == null)
                        _fpHelper = new FloatPriceHelper();
                    entity.Price = _fpHelper.GetFloatPrice(pf.OrganizationID, entity.Style.BYQID, entity.Style.Price);
                };
                win.ShowDialog();
            }
            else
                MessageBox.Show("请先指定款号");
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            var entity = btn.DataContext as CertificationBO;
            if (entity.ID == default(int))
            {
                MessageBox.Show("请先保存.");
                return;
            }
            entity.GradeName = _dataContext.Grades.First(o => o.ID == entity.Grade).Name;
            entity.SafetyTechniqueName = _dataContext.SafetyTechs.First(o => o.ID == entity.SafetyTechnique).Name;
            entity.CarriedStandardName = _dataContext.CarriedStandards.First(o => o.ID == entity.CarriedStandard).Name;
            CertificationPrintSetWin win = new CertificationPrintSetWin();
            win.DataContext = new { Certification = entity, PrintTicket = new CertificationPrintTicket() };
            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.ShowDialog();
        }

        private void btnSizeSave_Click(object sender, RoutedEventArgs e)
        {
            var result = _dataContext.SaveSize();
            MessageBox.Show(result.Message);
        }
    }

    public class FloatPriceConverter : IMultiValueConverter
    {
        FloatPriceHelper _fpHelper = new FloatPriceHelper();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int pid = (int)values[0];
                int oid = (int)values[1];
                return _fpHelper.GetFloatPrice(oid, pid).ToString();//不转成string，竟然就不能绑定到前台UI控件的Text属性上，不知道这是MultiBinding特有的还是Binding也有这个情况
            }
            catch
            {
                return "0";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CompositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new ObservableCollection<MaterielInfo>();
            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return new ObservableCollection<MaterielInfo>();
            return SerializeHelper.ObjectXml<ObservableCollection<MaterielInfo>>(str, Encoding.Unicode);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            return SerializeHelper.XmlObject(value);
        }
    }

    public class MaterielInfo
    {
        public string KindName { get; set; }

        private ObservableCollection<MaterielPercent> _materielPercents;
        public ObservableCollection<MaterielPercent> MaterielPercents
        {
            get
            {
                if (_materielPercents == null)
                {
                    _materielPercents = new ObservableCollection<MaterielPercent>();
                }
                return _materielPercents;
            }
        }
    }

    public class MaterielPercent
    {
        public decimal Percent { get; set; }
        public string MaterielName { get; set; }
    }
}
