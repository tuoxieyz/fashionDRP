using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;

namespace HabilimentERP
{
    public interface IWidget : INotifyPropertyChanged
    {

        WidgetState State { get; set; }

        BitmapImage Icon { get; }

        /// <summary>
        /// 是否在任务栏上锁定
        /// </summary>
        bool LockInBar { get; set; }
    }
}
