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
using ERPViewModelBasic;
using ManufacturingModel;
using View.Extension;
using Manufacturing.ViewModel;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for OuterFactorySelector.xaml
    /// </summary>
    public partial class OuterFactorySelector : UserControl
    {
        private bool _isUserEdited = false;//用户是否手动编辑了文本框

        public static readonly DependencyProperty IDValueProperty =
        DependencyProperty.Register("IDValue", typeof(int?), typeof(OuterFactorySelector), new PropertyMetadata(null, null, new CoerceValueCallback(OnIDValueCoerced)));

        public int? IDValue
        {
            get { return (int?)GetValue(IDValueProperty); }
            set { SetValue(IDValueProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable<Factory>), typeof(OuterFactorySelector));

        public IEnumerable<Factory> ItemsSource
        {
            get
            {
                var source = (IEnumerable<Factory>)GetValue(ItemsSourceProperty);
                if (source == null)
                {
                    source = OuterFactoryVM.GetEnabledFactories();
                    SetValue(ItemsSourceProperty, source);
                }
                return source;
            }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public OuterFactorySelector()
        {
            InitializeComponent();
        }

        private void imgShowWin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ItemsSource.Count() == 0)
            {
                MessageBox.Show("没有可供选择的机构");
                return;
            }
            OuterFactorySelectorWin win = new OuterFactorySelectorWin(this.ItemsSource);
            win.OuterFactorySelected += new Action<Factory>(win_OuterFactorySelected);

            win.Closed += delegate
            {

                if (_isUserEdited && IDValue != 0)
                {
                    txtCodeName.Clear();
                    var factory = this.ItemsSource.FirstOrDefault(org => org.ID == IDValue);
                    if (factory != null)
                    {
                        txtCodeName.Text = "[" + factory.Code + "]" + factory.Name;
                        _isUserEdited = false;
                    }
                }
            };

            win.Owner = UIHelper.GetAncestor<Window>(this);
            win.ShowDialog();
        }

        void win_OuterFactorySelected(Factory obj)
        {
            txtCodeName.Text = "[" + obj.Code + "]" + obj.Name;
            IDValue = obj.ID;
            _isUserEdited = false;
        }

        //Binding导致的属性更改也会触发强制回调
        private static object OnIDValueCoerced(DependencyObject d, object obj)
        {
            var os = (OuterFactorySelector)d;
            os.txtCodeName.Text = "";
            var orgs = os.ItemsSource;
            if (obj != null)
            {
                int oid = (int)obj;
                var factory = orgs.FirstOrDefault(org => org.ID == oid);
                if (factory != null)
                {
                    os.txtCodeName.Text = "[" + factory.Code + "]" + factory.Name;
                    os._isUserEdited = false;//私有变量也能这么访问？
                }
            }
            return obj;
        }

        private void txtCodeName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)//回车
            {
                var orgs = this.ItemsSource;
                var orgsFound = orgs.Where(org => org.Code == txtCodeName.Text);
                if (orgsFound != null && orgsFound.Count() == 1)
                {
                    var org = orgsFound.First();
                    IDValue = org.ID;
                }
                else
                {
                    imgShowWin_MouseLeftButtonUp(null, null);
                }
            }
            else
                _isUserEdited = true;
        }

        private void txtCodeName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtCodeName.SelectAll();
        }
    }
}
