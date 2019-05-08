using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;
using DBAccess;
using Telerik.Windows.Controls;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class MenuTreeVM : INotifyPropertyChanged
    {
        private List<QSModuleTreeItem> _qsModuleTreeItems = new List<QSModuleTreeItem>();

        public List<ModuleTreeItem> ModuleTreeItems { get; private set; }
        public List<QSModuleTreeItem> QSModuleTreeItems
        {
            get
            {
                return _qsModuleTreeItems;
            }
        }

        public MenuTreeVM()
        {
            ModuleTreeItems = RoleVM.ModuleTreeItems;
            OnPropertyChanged("ModuleTreeItems");
        }

        public ICommand QSearchCommand
        {
            get
            {
                return new DelegateCommand(param =>
                {
                    var txt = (string)param;
                    if (string.IsNullOrEmpty(txt))
                        return;
                    //_qsModuleTreeItems.Clear();//这样无用
                    _qsModuleTreeItems = new List<QSModuleTreeItem>();
                    ModuleTreeItems.ForEach(m =>
                    {
                        SearchMenu(m, txt);
                    });
                    OnPropertyChanged("QSModuleTreeItems");
                });
            }
        }

        private void SearchMenu(ModuleTreeItem module, string txt)
        {
            if (module.Children != null)
                module.Children.ForEach(m =>
                {
                    SearchMenu(m, txt);               
                });
            else
            {
                if (module.Module.Name.Contains(txt))
                {
                    _qsModuleTreeItems.Add(new QSModuleTreeItem
                    {
                        Icon = module.Icon,
                        Module = module.Module,
                        Tip = GetAncestorPath(module.Module) + module.Module.Name
                    });
                }
            }
        }

        private string GetAncestorPath(SysModule m)
        {
            string ret = "";
            if (m.ParentCode != "root")
            {
                var pm = RoleVM.SysModules.Find(sm => sm.Code == m.ParentCode);
                ret = GetAncestorPath(pm) + pm.Name + "﹥";
            }
            return ret;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
