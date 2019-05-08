using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using SysProcessModel;
using System.Transactions;
using Kernel;
using DistributionModel;

namespace DomainLogicEncap
{
    public static class UserLogic
    {
        private static QueryGlobal _query = new QueryGlobal("SysProcessConstr");
        private static QueryGlobal _queryDistribution = new QueryGlobal("DistributionConstr");

        /// <summary>
        /// 根据用户ID获取相应的菜单权限
        /// </summary>
        /// <param name="userID">用户ID</param>
        public static List<SysModule> ModuleProcessOfUser(int userID)
        {
            var urs = _query.QueryProvider.GetTable<SysUserRole>("SysUserRole");
            var rms = _query.QueryProvider.GetTable<SysRoleModule>("SysRoleModule");
            var ms = _query.QueryProvider.GetTable<SysModule>("SysModule");
            var query = from ur in urs
                        from rm in rms
                        where ur.RoleId == rm.RoleId && ur.UserId == userID
                        from m in ms
                        where m.ID == rm.ModuleId
                        select m;
            return query.ToList().Distinct(new IDEntityComparer<SysModule>()).ToList();
        }

        /// <summary>
        /// 获取用户的菜单权限
        /// </summary>
        /// <returns>菜单集合</returns>
        public static List<SysModule> GetModuleProcesses(this SysUser user)
        {
            return ModuleProcessOfUser(user.ID);
        }

        /// <summary>
        /// 根据用户ID获取用户所属的角色集
        /// </summary>
        /// <param name="userID">角色集合</param>
        public static List<SysRole> GetRolesOfUser(int userID)
        {
            var urs = _query.QueryProvider.GetTable<SysUserRole>("SysUserRole");
            var rs = _query.QueryProvider.GetTable<SysRole>("SysRole");
            var query = from r in rs
                        from ur in urs
                        where r.ID == ur.RoleId && ur.UserId == userID
                        select r;
            return query.ToList();
        }

        public static SysUser GetUserWhenLogin(string userCode, string password)
        {
            password = password.ToMD5String();
            var user = _query.LinqOP.Search<SysUser>(u => u.Code == userCode && u.Password == password).FirstOrDefault();
            return user;
        }
    }
}
