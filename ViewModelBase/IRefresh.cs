using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewModelBasic
{
    /// <summary>
    /// 刷新内存数据，使数据保持最新
    /// </summary>
    public interface IRefresh
    {
        void Refresh();
    }
}
