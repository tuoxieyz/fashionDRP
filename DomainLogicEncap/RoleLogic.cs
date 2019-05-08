using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;
using DBAccess;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace DomainLogicEncap
{
    public static class RoleLogic
    {
        private static QueryGlobal _query = new QueryGlobal("SysProcessConstr");

        public static SysRole GetRole(int rid)
        {
            var rs = _query.LinqOP.Search<SysRole>(r => r.ID == rid).ToList();
            if (rs.Count == 0)
                return null;
            return rs[0];
        }

        /// <summary>
        /// 根据角色ID获取相应的菜单权限
        /// </summary>
        /// <param name="roleID">角色ID</param>
        public static List<SysModule> ModuleProcessOfRole(int roleID)
        {
            var rms = _query.QueryProvider.GetTable<SysRoleModule>("SysRoleModule");
            var ms = _query.QueryProvider.GetTable<SysModule>("SysModule");
            var query = from rm in rms
                        from m in ms
                        where m.ID == rm.ModuleId && rm.RoleId == roleID
                        select m;
            return query.ToList();
        }
    }
}
