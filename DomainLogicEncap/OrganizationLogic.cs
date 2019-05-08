using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using System.Transactions;
using System.Configuration;
using SysProcessModel;
using DistributionModel;

namespace DomainLogicEncap
{
    public static class OrganizationLogic
    {
        private static QueryGlobal _query = new QueryGlobal("SysProcessConstr");
        public static QueryGlobal QueryDistribution = new QueryGlobal("DistributionConstr");

        public static QueryGlobal Query
        {
            get { return _query; }
        }

        /// <summary>
        /// 获取指定组织机构的用户集合
        /// </summary>
        /// <param name="oid">组织机构ID</param>
        /// <returns>用户集合</returns>
        public static List<SysUser> GetUsersOfOrgnization(int oid)
        {
            return _query.LinqOP.Search<SysUser>(u => u.OrganizationID == oid).ToList();
        }

        /// <summary>
        /// 获取机构的所有用户
        /// </summary>
        /// <returns>用户集合</returns>
        public static List<SysUser> GetAllUsers(this SysOrganization org)
        {
            return GetUsersOfOrgnization(org.ID);
        }

        /// <summary>
        /// 获取指定组织机构的角色集合
        /// </summary>
        /// <param name="oid">组织机构ID</param>
        /// <returns>角色集合</returns>
        public static List<SysRole> GetRolesOfOrgnization(int oid)
        {
            return _query.LinqOP.Search<SysRole>(r => r.OrganizationID == oid).ToList();
        }

        /// <summary>
        /// 获取机构的所有角色
        /// </summary>
        /// <returns>角色集合</returns>
        public static List<SysRole> GetAllRoles(this SysOrganization org)
        {
            return GetRolesOfOrgnization(org.ID);
        }

        /// <summary>
        /// 获取下级机构集合
        /// </summary>
        /// <param name="oid">本机机构ID</param>
        /// <param name="containSelfRunShop">是否包含自营店</param>
        /// <param name="all">是否不考虑机构状态（即包含已禁用机构）</param>
        /// <returns>下级机构集合</returns>
        public static List<SysOrganization> GetChildOrganizations(int oid, bool containSelfRunShop = true, bool all = false)
        {
            var orgs = _query.LinqOP.Search<SysOrganization>(o => o.ParentID == oid);
            if (!containSelfRunShop)
            {
                var sstype = _query.LinqOP.Search<SysOrganizationType>(o => o.OrganizationID == oid && o.Name == "自营店").FirstOrDefault();
                if (sstype != null)
                    orgs = orgs.Where(o => o.TypeId != sstype.ID);
            }
            if (!all)
                orgs = orgs.Where(o => o.Flag);
            return orgs.ToList();
        }

        /// <summary>
        /// 获取拥有指定品牌权限的下级机构集合
        /// </summary>
        /// <param name="oid">本机机构ID</param>
        /// <param name="bid">品牌ID</param>
        /// <param name="containSelfRunShop">是否包含自营店</param>
        /// <param name="all">是否不考虑机构状态（即包含已禁用机构）</param>
        /// <returns>下级机构集合</returns>
        public static List<SysOrganization> GetChildOrganizations(int oid, int bid, bool containSelfRunShop = true, bool all = false)
        {
            var os = _query.QueryProvider.GetTable<SysOrganization>("SysOrganization");
            //var obs = QueryDistribution.QueryProvider.GetTable<OrganizationBrand>("Distribution.dbo.OrganizationBrand");
            //作用同下句,两句作用一样，可得出内部拼接查询字符串是根据方法参数Distribution.dbo.OrganizationBrand，而非类型参数OrganizationBrand
            var obs = _query.QueryProvider.GetTable<OrganizationBrand>("OrganizationBrand");
            var query = from o in os
                        from ob in obs
                        where o.ID == ob.OrganizationID && o.ParentID == oid && ob.BrandID == bid
                        select o;
            if (!containSelfRunShop)
            {
                var sstype = _query.LinqOP.Search<SysOrganizationType>(o => o.OrganizationID == oid && o.Name == "自营店").FirstOrDefault();
                if (sstype != null)
                    query = query.Where(o => o.TypeId != sstype.ID);
            }
            if (!all)
                query = query.Where(o => o.Flag);
            return query.ToList();
        }

        /// <summary>
        /// 获取同级机构
        /// </summary>
        public static List<SysOrganization> GetSiblingOrganizations(int oid)
        {
            var parentID = _query.LinqOP.Search<SysOrganization,int>(selector:o=>o.ParentID,condition: o => o.ID == oid).FirstOrDefault();
            return GetChildOrganizations(parentID);
        }

        /// <summary>
        /// 获取同级店铺
        /// <remarks>目前并没有字段表明是否店铺,是否店铺根据机构名称最后一个字是否是"店"来判断</remarks>
        /// </summary>
        public static List<SysOrganization> GetSiblingShops(int oid)
        {
            var parentID = _query.LinqOP.Search<SysOrganization, int>(selector: o => o.ParentID, condition: o => o.ID == oid).FirstOrDefault();
            var orgs = _query.LinqOP.Search<SysOrganization>(o => o.ParentID == parentID && o.Name.EndsWith("店") && o.Flag);
            return orgs.ToList();
        }

        public static void BatchSaveContractDiscount(List<OrganizationContractDiscount> cds)
        {
            var lp = QueryDistribution.LinqOP;
            //用cds.ForEach(cd =>//则cd = os[0]这一步不行
            //foreach(var cd in cds)//这也会在给cd赋值时报错
            for (int i = 0; i < cds.Count; i++)
            {
                var cd = cds[i];
                var os = lp.Search<OrganizationContractDiscount>(o => o.OrganizationID == cd.OrganizationID && o.BYQID == cd.BYQID).ToList();
                if (os.Count != 0)//如果相关合同折扣已被设置
                {
                    os[0].Discount = cd.Discount;
                    cds[i] = os[0];//千万不能用cd=os[0],可能上述两点也是由于这个问题——引用指针问题
                }
            }
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.AddOrUpdate<OrganizationContractDiscount>(cds);
                    scope.Complete();
                }
                catch
                {
                    throw;
                }
            }
        }

        public static void BatchSavePriceFloat(List<OrganizationPriceFloat> cds)
        {
            var lp = _query.LinqOP;
            for (int i = 0; i < cds.Count; i++)
            {
                var cd = cds[i];
                var os = lp.Search<OrganizationPriceFloat>(o => o.OrganizationID == cd.OrganizationID && o.BYQID == cd.BYQID).ToList();
                if (os.Count != 0)//如果相关合同折扣已被设置
                {
                    os[0].FloatRate = cd.FloatRate;
                    os[0].LastNumber = cd.LastNumber;
                    cds[i] = os[0];//千万不能用cd=os[0],可能上述两点也是由于这个问题——引用指针问题
                }
            }
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    lp.AddOrUpdate<OrganizationPriceFloat>(cds);
                    scope.Complete();
                }
                catch
                {
                    throw;
                }
            }
        }

        public static void AddDefaultStorage(SysOrganization organization)
        {
            QueryDistribution.LinqOP.Add<Storage>(new Storage
            {
                OrganizationID = organization.ID,
                Name = "成品仓库",
                CreatorID = organization.CreatorID,
                CreateTime = organization.CreateTime
            });
        }

        public static List<Storage> GetStorages(int oid)
        {
            return QueryDistribution.LinqOP.Search<Storage>(o => o.OrganizationID == oid && o.Flag).ToList();
        }

        public static OrganizationContractDiscount GetOrganizationContractDiscount(int byqID, int organizationID)
        {
            var discount = QueryDistribution.LinqOP.Search<OrganizationContractDiscount>(o => o.OrganizationID == organizationID && o.BYQID == byqID).FirstOrDefault();
            return discount;
        }

        public static List<SysArea> GetAreas()
        {
            return _query.LinqOP.Search<SysArea>().ToList();
        }

        public static List<SysProvience> GetProvinces()
        {
            return _query.LinqOP.Search<SysProvience>().ToList();
        }

        public static List<SysCity> GetCities()
        {
            return _query.LinqOP.Search<SysCity>().ToList();
        }

        /// <summary>
        /// 取得某组织机构为下级机构创建的所有机构类型
        /// </summary>
        public static List<SysOrganizationType> GetOrganizationTypes(int oid)
        {
            return _query.LinqOP.Search<SysOrganizationType>(o => o.OrganizationID == oid && o.IsEnabled).ToList();
        }
    }
}
