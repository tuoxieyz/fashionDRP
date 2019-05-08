using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HabilimentERP
{
    internal class TriggerBasePrivoder
    {
        internal List<TriggerBase> BarTriggers { get; set; }

        internal TriggerBasePrivoder()
        {
            BarTriggers = new List<TriggerBase>();
        }
    }
}
