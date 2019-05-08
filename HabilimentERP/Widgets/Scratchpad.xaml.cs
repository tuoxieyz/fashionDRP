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
using System.ComponentModel;
using HabilimentERP.Gestures;
using View.Extension;

namespace HabilimentERP
{
    /// <summary>
    /// Interaction logic for Scratchpad.xaml
    /// </summary>
    public partial class Scratchpad : UserControl, IWidget
    {
        public Scratchpad()
        {
            InitializeComponent();

            this.Loaded += delegate
            {
                new Pan().Invest(this, this);
            };
        }

        public WidgetState State
        {
            get
            {
                return WidgetHelper.GetState(this);
            }
            set
            {
                if (value != State)
                {
                    WidgetHelper.SetState(this, value);
                    OnPropertyChanged("State");
                }
            }
        }

        public BitmapImage Icon
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/Images/BottomBar/note.png", UriKind.Relative));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        bool _lockInBar;

        public bool LockInBar
        {
            get
            {
                return _lockInBar;
            }
            set
            {
                _lockInBar = value;
            }
        }
    }
}
