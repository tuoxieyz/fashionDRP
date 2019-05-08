using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ModuleTreeItem
    {
        public SysModule Module { get; set; }
        public List<ModuleTreeItem> Children { get; set; }
        /// <summary>
        /// 图标地址
        /// </summary>
        public string Icon { get; set; }
        public bool IsExpanded { get; set; }
    }

    /// <summary>
    /// 快速查找的菜单项
    /// </summary>
    public class QSModuleTreeItem
    {
        public SysModule Module { get; set; }
        public string Icon { get; set; }
        /// <summary>
        /// 描述上下级关系
        /// </summary>
        public string Tip { get; set; }
    }

    /// <summary>
    /// 用于角色设置的菜单项
    /// </summary>
    public class RoleSetModuleTreeItem : ModuleTreeItem
    {
        public new List<RoleSetModuleTreeItem> Children { get; set; }
        public bool IsChecked { get; set; }
    }
}
