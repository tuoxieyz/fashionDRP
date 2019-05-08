using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using ViewModelBasic;
using SysProcessModel;
using Kernel;
using DomainLogicEncap;


namespace SysProcessViewModel
{
    public class SysUserBO : SysUser, IDataErrorInfo
    {
        private DataChecker _checker;
        private const string _initpwd = "0secret";

        #region 属性

        private string _password = _initpwd;
        public override string Password { get { return _password; } set { _password = value; } }

        public DateTime CreateDate
        {
            get
            {
                return CreateTime.Date;
            }
        }

        private IEnumerable<SysRole> _roles;
        public IEnumerable<SysRole> Roles
        {
            get
            {
                if (_roles == null)
                {
                    _roles = UserLogic.GetRolesOfUser(this.ID);
                }
                return _roles;
            }
            set
            {
                _roles = value;
            }
        }

        private List<ProBrand> _brands;
        /// <summary>
        /// 该用户和当前登录用户品牌权限交集
        /// </summary>
        public List<ProBrand> Brands
        {
            get
            {
                if (_brands == null)
                {
                    var brandIDs = VMGlobal.SysProcessQuery.LinqOP.Search<UserBrand>(o => o.UserID == this.ID).Select(o => o.BrandID).ToArray();
                    _brands = VMGlobal.PoweredBrands.FindAll(o => brandIDs.Contains(o.ID)).ToList();
                }
                return _brands;
            }
            set
            {
                _brands = value;
            }
        }

        /// <summary>
        /// 报表查询时使用，业务逻辑无关
        /// </summary>
        public int RoleID { get; set; }

        public string RoleNames
        {
            get
            {
                string rolenames = "";
                foreach (var role in Roles)
                {
                    rolenames += (role.Name + ",");
                }
                return rolenames.TrimEnd(',');
            }
        }

        public BasicInfoEnum OperateAccess
        {
            get
            {
                var accesses = Roles.Select(r => r.OPAccess);
                BasicInfoEnum bi = (BasicInfoEnum)0;//虽然0没有对应任何BasicInfoEnum，但这种方式可行
                foreach (var access in accesses)
                {
                    bi = bi | (BasicInfoEnum)access;
                }
                return bi;
            }
        }

        public IMReceiveAccessEnum IMReceiveAccess
        {
            get
            {
                var accesses = Roles.Select(r => r.IMAccess);
                IMReceiveAccessEnum bi = (IMReceiveAccessEnum)0;
                foreach (var access in accesses)
                {
                    bi = bi | (IMReceiveAccessEnum)access;
                }
                return bi;
            }
        }

        public string BrandNames
        {
            get
            {
                string brandnames = "";
                foreach (var brand in Brands)
                {
                    brandnames += (brand.Name + ",");
                }
                return brandnames.TrimEnd(',');
            }
        }

        #endregion

        public SysUserBO()
        { }

        public SysUserBO(SysUser user)
        {
            this.ID = user.ID;
            this.Name = user.Name;
            this.Code = user.Code;
            this.OrganizationID = user.OrganizationID;
            this.Password = user.Password;
            this.Flag = user.Flag;
            CreateTime = user.CreateTime;
            CreatorID = user.CreatorID;
        }

        string IDataErrorInfo.Error
        {
            get { return ""; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return this.CheckData(columnName);
            }
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Code" || columnName == "Name")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.SysProcessQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataCodeName<SysUser>(this, columnName);
            }
            else if (columnName == "Password")
            {
                if (string.IsNullOrWhiteSpace(Password))
                    errorInfo = "不能为空";
                else if (!IsMatchPWDReg(Password))
                    errorInfo = "必须为至少6位字母和数字组合字符串";
            }
            return errorInfo;
        }

        /// <summary>
        /// 密码规则校验
        /// </summary>
        public static bool IsMatchPWDReg(string password)
        {
            //前面两个零宽非断言表示不能全为数字或全为字母
            string regex = @"^(?![0-9]+$)(?![a-zA-Z]+$)[A-Za-z0-9]{6,}$";
            return password.IsMatch(regex);
        }

        public OPResult ResetPassword(string initpwd = _initpwd)
        {
            this.Password = initpwd.ToMD5String();
            try
            {
                VMGlobal.SysProcessQuery.LinqOP.Update<SysUser>(this);
                return new OPResult { IsSucceed = true, Message = "操作成功,重置后密码为" + initpwd };
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "操作失败,失败原因:\n" + e.Message };
            }
        }
    }
}
