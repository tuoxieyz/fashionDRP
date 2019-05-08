using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ViewModelBasic
{
    /// <summary>
    /// 该类型一般用于在UI界面上可选择的项
    /// </summary>
    /// <typeparam name="T">实体类</typeparam>
    public class HoldableEntity<T> : INotifyPropertyChanged where T : class
    {
        private bool _isHold = false;

        public bool IsHold
        {
            get { return _isHold; }
            set
            {
                if (_isHold != value)
                {
                    _isHold = value;
                    OnPropertyChanged("IsHold");
                }
            }
        }

        public T Entity { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
