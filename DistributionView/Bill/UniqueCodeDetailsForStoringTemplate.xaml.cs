using DistributionViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace DistributionView.Bill
{
    /// <summary>
    /// UniqueCodeDetailsForStoringTemplate.xaml 的交互逻辑
    /// </summary>
    public partial class UniqueCodeDetailsForStoringTemplate : UserControl
    {
        public UniqueCodeDetailsForStoringTemplate()
        {
            InitializeComponent();
            this.Loaded += UniqueCodeDetailsForStoringTemplate_Loaded;
        }

        void UniqueCodeDetailsForStoringTemplate_Loaded(object sender, RoutedEventArgs e)
        {
#if UniqueCode
            var product = this.DataContext as ProductForStoringWhenReceiving;
            var source = product.UniqueCodes.Select(o => new UniqueCodeStateForStoring
            {
                UniqueCode = o
            }).ToList();//不ToList的后果是每次使用都去重新Select，即使使用同一个指针source，也可能引用的是不同的对象，这会导致预期之外的问题。并导致重复对象占用内存。
            icUniqueCode.ItemsSource = source;
            foreach (var item in product.ReceivedUniqueCodes)
            {
                var ucode = source.FirstOrDefault(o => o.UniqueCode == item);
                if (ucode != null)
                    ucode.IsChecked = true;
            }
            product.ReceivedUniqueCodes.CollectionChanged += (ss, ee) =>
            {
                if (ee.NewItems != null)
                {
                    foreach (var item in ee.NewItems)
                    {
                        var code = (string)item;
                        var ucode = source.FirstOrDefault(o => o.UniqueCode == code);
                        if (ucode != null)
                            ucode.IsChecked = true;
                    }
                }
                if (ee.OldItems != null)
                {
                    foreach (var item in ee.OldItems)
                    {
                        var code = (string)item;
                        var ucode = source.FirstOrDefault(o => o.UniqueCode == code);
                        if (ucode != null)
                            ucode.IsChecked = false;
                    }
                }
            };
#endif
        }
    }

    public class UniqueCodeStateForStoring : INotifyPropertyChanged
    {
        public string UniqueCode { get; set; }

        private bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
