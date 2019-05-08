using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;

using Telerik.Windows.Controls;
using DomainLogicEncap;
using Telerik.Windows.Data;


using System.Transactions;

using System.ComponentModel;
using SysProcessModel;
using System.Data;
using Kernel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using ViewModelBasic;
using System.Net.Http;
using System.Configuration;
using UpdateOnline;
using System.Diagnostics;

namespace SysProcessViewModel
{
    public class UserVM : PagedEditSynchronousVM<SysUser>
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
                        new ItemPropertyDefinition { DisplayName = "编号", PropertyName = "Code", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "名称", PropertyName = "Name", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Flag", PropertyType = typeof(Boolean)},
                        new ItemPropertyDefinition { DisplayName = "创建时间", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "角色", PropertyName = "RoleID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("Code", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Flag", FilterOperator.IsEqualTo, true)
                    };
                }
                return _filterDescriptors;
            }
        }

        private List<SysRole> _rolesCurrentUserCover;

        /// <summary>
        /// 当前登录用户所能支配的角色集合(角色所对应的权限当前用户都拥有的话，这个角色就包含其中)
        /// </summary>
        public List<SysRole> RolesCurrentUserCover
        {
            get
            {
                if (_rolesCurrentUserCover == null)
                {
                    DataSet ds = VMGlobal.SysProcessQuery.DB.ExecuteDataSet("GetRolesUserCover", VMGlobal.CurrentUser.ID);
                    _rolesCurrentUserCover = ds.Tables[0].ToList<SysRole>();
                }
                return _rolesCurrentUserCover;
            }
        }

        private List<SysUser> _usersOfCurrentOrgnization;

        /// <summary>
        /// 当前登录用户所在组织机构的用户集合
        /// </summary>
        public List<SysUser> UsersOfCurrentOrgnization
        {
            get
            {
                if (_usersOfCurrentOrgnization == null)
                    _usersOfCurrentOrgnization = OrganizationLogic.GetUsersOfOrgnization(VMGlobal.CurrentUser.OrganizationID);
                return _usersOfCurrentOrgnization;
            }
        }

        #endregion

        public UserVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = new List<SysUserBO>();
        }

        protected override IEnumerable<SysUser> SearchData()
        {
            var users = LinqOP.GetDataContext<SysUser>();
            var userroles = LinqOP.GetDataContext<SysUserRole>();
            var ouIDs = users.Where(u => u.OrganizationID == VMGlobal.CurrentUser.OrganizationID).Select(o => o.ID);
            //返回的用户集合必须是本机构用户或本机构用户所创建的用户
            users = from u in users where ouIDs.Contains(u.ID) || ouIDs.Contains(u.CreatorID) select u;
            var data = from u in users
                       join ur in userroles on u.ID equals ur.UserId into urs
                       from v in urs.DefaultIfEmpty()
                       select new SysUserBO
                       {
                           ID = u.ID,
                           RoleID = v.RoleId,
                           CreateTime = u.CreateTime.Date,
                           Code = u.Code,
                           Name = u.Name,
                           Flag = u.Flag
                       };
            var uids = ((IQueryable<SysUserBO>)data.Where(FilterDescriptors)).Select(o => o.ID).ToArray();
            return users.Where(o => uids.Contains(o.ID)).Select(o => new SysUserBO(o)).ToList();
        }

        public override OPResult AddOrUpdate(SysUser entity)
        {
            SysUserBO user = (SysUserBO)entity;
            return user.ID == default(int) ? this.Add(user) : this.Update(user);
        }

        //用户对应的各种数据集合,避免在UI绑定时频繁操作数据库
        //private void RefreshAttachData()
        //{
        //    var users = (List<SysUser>)Users.SourceCollection;
        //    var uIDs = users.Select<SysUser, int>(u => u.ID);

        //    var urs = _query.QueryProvider.GetTable<SysUserRole>("SysUserRole");
        //    var queryUR = from ur in urs where uIDs.Contains(ur.UserId) select ur;
        //    UserRoles = queryUR.ToList();

        //    var ubs = _queryDistribution.QueryProvider.GetTable<UserBrand>("UserBrand");
        //    var queryUB = from ub in ubs where uIDs.Contains(ub.UserID) select ub;
        //    UserBrands = queryUB.ToList();

        //    var oIDs = users.Select<SysUser, int>(u => u.OrganizationID);
        //    Organizations = _query.LinqOP.Search<SysOrganization>(o => oIDs.Contains(o.ID)).ToList();
        //}

        private OPResult Add(SysUserBO user)
        {
            int? userpointlimit = null;
            try
            {
                userpointlimit = this.GetUserPointLimit();
                if(userpointlimit == null)
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n无法确认用户点数是否存在上限，请联系软件公司。" };
                if ((userpointlimit != -1) && userpointlimit <= LinqOP.Search<SysUser>().Count())
                {
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n超过用户点数限制，若要增加用户点数，请联系软件公司。" };
                }
            }
            catch(Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
            }
            string oginalPWD = user.Password;
            user.CreatorID = VMGlobal.CurrentUser.ID;
            user.Password = oginalPWD.ToMD5String();
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    int id = LinqOP.Add<SysUser, int>(user, r => r.ID);
                    user.ID = id;
                    List<SysUserRole> urs = new List<SysUserRole>();
                    foreach (var r in user.Roles)
                    {
                        SysUserRole ur = new SysUserRole
                        {
                            UserId = id,
                            RoleId = r.ID,
                            CreatorID = VMGlobal.CurrentUser.ID
                        };
                        urs.Add(ur);
                    }
                    LinqOP.Add<SysUserRole>(urs);

                    List<UserBrand> ubs = new List<UserBrand>();
                    foreach (var b in user.Brands)
                    {
                        UserBrand ub = new UserBrand
                        {
                            UserID = id,
                            BrandID = b.ID,
                            CreatorID = VMGlobal.CurrentUser.ID
                        };
                        ubs.Add(ub);
                    }
                    VMGlobal.SysProcessQuery.LinqOP.Add<UserBrand>(ubs);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "保存成功." };
                }
                catch (Exception e)
                {
                    user.ID = default(int);
                    user.Password = oginalPWD;
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
        }

        private int? GetUserPointLimit()
        {
            if (MainWindowVM.Current.CustomerInfo != null)
            {
                return MainWindowVM.Current.CustomerInfo.UserPointLimit;
            }
            return null;
        }

        private OPResult Update(SysUserBO user)
        {
            var roleIDs = RolesCurrentUserCover.Select(rs => rs.ID);//加上ToList()在下面执行时就抛出Contains不支持的异常，坑爹
            List<SysUserRole> urs = new List<SysUserRole>();
            foreach (var rs in user.Roles)
            {
                SysUserRole ur = new SysUserRole
                {
                    UserId = user.ID,
                    RoleId = rs.ID,
                    CreatorID = VMGlobal.CurrentUser.ID
                };
                urs.Add(ur);
            }

            var brandIDs = VMGlobal.PoweredBrands.Select(bs => bs.ID);
            List<UserBrand> ubs = new List<UserBrand>();
            foreach (var bs in user.Brands)
            {
                UserBrand ub = new UserBrand
                {
                    UserID = user.ID,
                    BrandID = bs.ID,
                    CreatorID = VMGlobal.CurrentUser.ID
                };
                ubs.Add(ub);
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Update<SysUser>(user);
                    LinqOP.Delete<SysUserRole>(ur => roleIDs.Contains(ur.RoleId) && ur.UserId == user.ID);
                    VMGlobal.SysProcessQuery.LinqOP.Delete<UserBrand>(ub => brandIDs.Contains(ub.BrandID) && ub.UserID == user.ID);
                    LinqOP.Add<SysUserRole>(urs);
                    VMGlobal.SysProcessQuery.LinqOP.Add<UserBrand>(ubs);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "更新成功." };
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "更新失败,失败原因:\n" + e.Message };
                }
            }
        }
    }
}
