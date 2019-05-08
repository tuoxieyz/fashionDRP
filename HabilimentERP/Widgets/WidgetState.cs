using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HabilimentERP
{
    /// <summary>
    /// 部件状态
    /// </summary>
    public enum WidgetState
    {
        Showed,//显示但并不是当前活动部件
        Actived,
        Mined,
        Closed
    }
}
