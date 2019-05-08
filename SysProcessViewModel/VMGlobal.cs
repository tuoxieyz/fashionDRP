using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainLogicEncap;
using DBAccess;
using SysProcessModel;
using ViewModelBasic;

namespace SysProcessViewModel
{
    public static class VMGlobal
    {
        public static readonly QueryGlobal SysProcessQuery = new QueryGlobal("SysProcessConstr");
        public static readonly QueryGlobal DistributionQuery = new QueryGlobal("DistributionConstr");
        public static readonly QueryGlobal ManufacturingQuery = new QueryGlobal("ManufacturingConnection");

        public static event Action RefreshingEvent;

        public static SysUserBO CurrentUser { get; set; }

        private static List<DataState> _states;

        public static List<DataState> States
        {
            get
            {
                if (_states == null)
                {
                    _states = new List<DataState>();
                    _states.Add(new DataState { Flag = true, Name = "正常" });
                    _states.Add(new DataState { Flag = false, Name = "禁用" });
                }
                return _states;
            }
        }

        private static List<DataState> _sex;

        public static List<DataState> Sex
        {
            get
            {
                if (_sex == null)
                {
                    _sex = new List<DataState>();
                    _sex.Add(new DataState { Flag = true, Name = "男" });
                    _sex.Add(new DataState { Flag = false, Name = "女" });
                }
                return _sex;
            }
        }

        public static List<SysOrganizationBO> ChildOrganizations
        {
            get
            {
                return OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            }
        }

        private static List<SysOrganizationType> _organizationTypes;
        /// <summary>
        /// 子级对应的机构类型集合
        /// </summary>
        public static List<SysOrganizationType> OrganizationTypes
        {
            get
            {
                if (_organizationTypes == null)
                {
                    _organizationTypes = OrganizationLogic.GetOrganizationTypes(CurrentUser.OrganizationID);
                }
                return _organizationTypes;
            }
        }

        #region 成品的各种属性

        private static List<ProBrand> _poweredBrands;

        /// <summary>
        /// 登录用户拥有的品牌权限和用户机构拥有的品牌权限的交集
        /// </summary>
        public static List<ProBrand> PoweredBrands
        {
            get
            {
                if (_poweredBrands == null)
                    _poweredBrands = GetPoweredBrands();
                return _poweredBrands;
            }
        }

        public static List<ProBrand> GetPoweredBrands(SysUser user)
        {
            var ubs = SysProcessQuery.LinqOP.Search<UserBrand>(ub => ub.UserID == user.ID);
            var obs = SysProcessQuery.LinqOP.Search<OrganizationBrand>(ob => ob.OrganizationID == user.OrganizationID);
            var brands = from ub in ubs
                         from ob in obs
                         where ub.BrandID == ob.BrandID
                         select ub.BrandID;
            var bids = brands.ToList().AsEnumerable();
            return SysProcessQuery.LinqOP.Search<ProBrand>(b => bids.Contains(b.ID)).ToList();
        }

        private static List<ProBrand> GetPoweredBrands()
        {
            return VMGlobal.GetPoweredBrands(VMGlobal.CurrentUser);
        }

        public static IEnumerable<ProBrand> AvailableBrands
        {
            get
            {
                return PoweredBrands.Where(o => o.Flag);
            }
        }

        private static List<ProBoduan> _boduans;

        private static List<ProQuarter> _quarters;

        private static List<ProName> _pronames;

        private static List<ProSize> _sizes;

        private static List<ProUnit> _units;

        private static List<ProColor> _colors;

        private static List<ProBYQ> _byqs;

        //品名
        public static List<ProName> ProNames
        {
            get
            {
                if (_pronames == null)
                {
                    _pronames = SysProcessQuery.LinqOP.Search<ProName>().ToList();
                }
                return _pronames;
            }
        }

        public static IEnumerable<ProName> AvailableProNames
        {
            get
            {
                return ProNames.Where(o => o.Flag);
            }
        }

        //波段
        public static List<ProBoduan> Boduans
        {
            get
            {
                if (_boduans == null)
                {
                    _boduans = SysProcessQuery.LinqOP.Search<ProBoduan>().ToList();
                }
                return _boduans;
            }
        }

        public static IEnumerable<ProBoduan> AvailableBoduans
        {
            get
            {
                return Boduans.Where(o => o.IsEnabled);
            }
        }

        //季度
        public static List<ProQuarter> Quarters
        {
            get
            {
                if (_quarters == null)
                {
                    _quarters = SysProcessQuery.LinqOP.Search<ProQuarter>().ToList();
                }
                return _quarters;
            }
        }

        public static IEnumerable<ProQuarter> AvailableQuarters
        {
            get
            {
                return Quarters.Where(o => o.IsEnabled);
            }
        }

        //尺码
        public static List<ProSize> Sizes
        {
            get
            {
                if (_sizes == null)
                {
                    _sizes = SysProcessQuery.LinqOP.Search<ProSize>().ToList();
                }
                return _sizes;
            }
        }

        //单位
        public static List<ProUnit> Units
        {
            get
            {
                if (_units == null)
                {
                    _units = SysProcessQuery.LinqOP.Search<ProUnit>().ToList();
                }
                return _units;
            }
        }

        public static IEnumerable<ProUnit> AvailableUnits
        {
            get
            {
                return Units.Where(o => o.Flag);
            }
        }

        //颜色
        public static List<ProColor> Colors
        {
            get
            {
                if (_colors == null)
                {
                    _colors = SysProcessQuery.LinqOP.Search<ProColor>().ToList();
                }
                return _colors;
            }
        }

        public static List<ProBYQ> BYQs
        {
            get
            {
                if (_byqs == null)
                {
                    IEnumerable<int> bids = PoweredBrands.Select(o => o.ID);
                    _byqs = SysProcessQuery.LinqOP.Search<ProBYQ>(o => bids.Contains(o.BrandID)).ToList();
                }
                return _byqs;
            }
        }

        #endregion

        /// <summary>
        /// 清空用户信息,包括品牌权限仓库信息等
        /// </summary>
        public static void CleanUserData()
        {
            Refresh();
            VMGlobal.CurrentUser = null;
        }

        public static void Refresh()
        {
            _poweredBrands = null;
            _pronames = null;
            _sizes = null;
            _units = null;
            _colors = null;
            _byqs = null;
            OrganizationListVM.CurrentOrganization = null;
            OrganizationListVM.CurrentAndChildrenOrganizations = null;
            _organizationTypes = null;
            RoleVM.RolesOfCurrentOrgnization = null;
            RoleVM.ModuleTreeItems = null;
            if (RefreshingEvent != null)
            {
                RefreshingEvent();
            }
        }
    }
}
