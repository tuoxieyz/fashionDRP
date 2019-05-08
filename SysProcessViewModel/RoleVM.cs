using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DBAccess;
using System.Transactions;
using DomainLogicEncap;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Kernel;
using System.Data;
using System.ComponentModel;
using SysProcessModel;
using Telerik.Windows.Controls.Data.DataFilter;
using ViewModelBasic;
using Telerik.Windows.Controls.Data.DataForm;

namespace SysProcessViewModel
{
    public class RoleVM : EditSynchronousVM<SysRole>
    {
        #region 属性

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "角色名称", PropertyName = "Name", PropertyType = typeof(string)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public override CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("Name", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, VMGlobal.CurrentUser.OrganizationID)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        static RoleVM()
        {
            VMGlobal.RefreshingEvent += delegate { _sysModules = null; };
        }

        public RoleVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = new List<SysRoleBO>();
        }

        protected override IEnumerable<SysRole> SearchData()
        {
            return base.SearchData().Select(o => new SysRoleBO(o)).ToList();
        }

        public override OPResult AddOrUpdate(SysRole entity)
        {
            SysRoleBO role = (SysRoleBO)entity;
            return role.ID == default(int) ? this.Add(role) : this.Update(role);
        }

        /// <summary>
        /// 删除角色，同时删除角色权限和用户角色映射关系
        /// </summary>
        /// <param name="rid">角色ID</param>
        /// <returns>删除是否成功</returns>
        public override OPResult Delete(SysRole entity)
        {
            int rid = entity.ID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Delete<SysRoleModule>(rm => rm.RoleId == rid);
                    VMGlobal.SysProcessQuery.DB.ExecuteNonQuery("CleanUpDirtyProcess", rid);
                    LinqOP.Delete<SysRole>(r => r.ID == rid);
                    LinqOP.Delete<SysUserRole>(ur => ur.RoleId == rid);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "删除成功!" };
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
                }
            }
        }

        #region 角色

        private static List<SysRole> _rolesOfCurrentOrgnization;

        /// <summary>
        /// 当前登录用户所在组织机构的角色集合
        /// </summary>
        public static List<SysRole> RolesOfCurrentOrgnization
        {
            get
            {
                if (_rolesOfCurrentOrgnization == null)
                    _rolesOfCurrentOrgnization = OrganizationLogic.GetRolesOfOrgnization(VMGlobal.CurrentUser.OrganizationID);
                return _rolesOfCurrentOrgnization;
            }
            set
            {
                _rolesOfCurrentOrgnization = value;
            }
        }

        private OPResult Update(SysRoleBO role)
        {
            List<SysRoleModule> rms = new List<SysRoleModule>();
            foreach (var m in role.Modules)
            {
                SysRoleModule rm = new SysRoleModule
                {
                    RoleId = role.ID,
                    ModuleId = m.ID
                };
                rms.Add(rm);
            }
            //该角色被删除的菜单权限，拥有该角色的用户的所有下级用户的相应菜单权限也要一起删除,若同样权限是其它角色赋予则不删除
            //var oldrms = _query.LinqOP.Search<SysRoleModule>(rm => rm.RoleId == role.ID).ToList();
            //oldrms.Except(rms);//原列表中有新列表中没有的权限映射(待级联删除的)
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Update<SysRole>(role);
                    LinqOP.Delete<SysRoleModule>(rm => rm.RoleId == role.ID);
                    LinqOP.Add<SysRoleModule>(rms);
                    VMGlobal.SysProcessQuery.DB.ExecuteNonQuery("CleanUpDirtyProcess", role.ID);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "更新失败,失败原因:\n" + e.Message };
                }
            }
            if (_rolesOfCurrentOrgnization != null)
            {
                var roleUpdate = _rolesOfCurrentOrgnization.FirstOrDefault(r => r.ID == role.ID);
                if (roleUpdate != null)
                {
                    int index = _rolesOfCurrentOrgnization.IndexOf(roleUpdate);
                    _rolesOfCurrentOrgnization[index] = role; //roleUpdate = role;
                }
            }
            return new OPResult { IsSucceed = true, Message = "更新成功." };
        }

        private OPResult Add(SysRoleBO role)
        {
            role.CreatorID = VMGlobal.CurrentUser.ID;
            role.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    int id = LinqOP.Add<SysRole, int>(role, r => r.ID);
                    role.ID = id;
                    List<SysRoleModule> rms = new List<SysRoleModule>();
                    foreach (var m in role.Modules)
                    {
                        SysRoleModule rm = new SysRoleModule
                        {
                            RoleId = id,
                            ModuleId = m.ID
                        };
                        rms.Add(rm);
                    }
                    LinqOP.Add<SysRoleModule>(rms);
                    if (_rolesOfCurrentOrgnization != null)//新增成功后同步更新_rolesOfCurrentOrgnization
                        _rolesOfCurrentOrgnization.Add(role);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "保存成功." };
                }
                catch (Exception e)
                {
                    role.ID = default(int);
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
        }

        #endregion

        private static List<SysModule> _sysModules;

        /// <summary>
        /// 当前登录用户的模块权限
        /// </summary>
        public static List<SysModule> SysModules
        {
            get
            {
                if (_sysModules == null)
                {
                    _sysModules = UserLogic.ModuleProcessOfUser(VMGlobal.CurrentUser.ID);
                }
                return _sysModules;
            }
        }

        #region 构造UI使用的菜单集合

        private static List<ModuleTreeItem> _moduleTreeItems;
        public static List<ModuleTreeItem> ModuleTreeItems
        {
            get
            {
                if (_moduleTreeItems == null)
                {
                    _moduleTreeItems = ChangeSysModuleToTreeItem(SysModules);
                }
                return _moduleTreeItems;
            }
            set
            {
                _moduleTreeItems = value;
            }
        }

        public static List<ModuleTreeItem> ChangeSysModuleToTreeItem(List<SysModule> modules)
        {
            var moduleTreeItems = new List<ModuleTreeItem>();
            var roots = modules.FindAll(m => m.ParentCode == "root");
            roots.ForEach(root =>
            {
                var treeItem = ApplyChildrenForModule(root, modules);
                moduleTreeItems.Add(treeItem);
            });
            return moduleTreeItems;
        }

        /// <summary>
        /// 递归构造菜单树节点
        /// </summary>
        private static ModuleTreeItem ApplyChildrenForModule(SysModule module, List<SysModule> modules)
        {
            ModuleTreeItem treeItem = new ModuleTreeItem();

            treeItem.Module = module;
            var children = modules.FindAll(m => m.ParentCode == module.Code);
            if (children.Count > 0)
            {
                var moduleTreeItems = new List<ModuleTreeItem>();
                children.ForEach(m =>
                {
                    moduleTreeItems.Add(ApplyChildrenForModule(m, modules));
                });
                treeItem.Children = moduleTreeItems;
            }
            //下句在xaml中可用，在后台代码中不能定位，另外需要注意相对路径的深度不要搞错
            //treeItem.Icon = "../Images/Menu/" + SetMenuIcon(treeItem.Module.Name);
            //下句更具通用性,在后台代码和xaml中都可用，且没有相对路径的深度麻烦
            treeItem.Icon = @"pack://application:,,,/HabilimentERP;component/Images/Menu/" + SetMenuIcon(treeItem.Module.Name);
            return treeItem;
        }

        private static string SetMenuIcon(string menuName)
        {
            string icon = menuName + ".png";
            if (menuName.EndsWith("单"))
                icon = "制单.png";
            else if (menuName.EndsWith("分布"))
                icon = "分布.png";
            else if (menuName.EndsWith("计划"))
                icon = "计划.png";
            else if (menuName.EndsWith("功能"))
                icon = "工具.png";
            else if (menuName.EndsWith("汇总"))
                icon = "汇总.png";
            else if (menuName.EndsWith("查询"))
                icon = "查询.png";
            else if (menuName.EndsWith("轨迹"))
                icon = "轨迹.png";
            return icon;
        }

        #endregion

        public static DataFormCommandButtonsVisibility GetCurrentUserDataFormCommand(BasicInfoEnum bi)
        {
            var op = DataFormCommandButtonsVisibility.Navigation;
            if ((VMGlobal.CurrentUser.OperateAccess & bi) == bi)
                op = DataFormCommandButtonsVisibility.All;
            return op;
        }

    }
}
