using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Automation;
using DomainLogicEncap;
using SysProcessModel;
using System.Globalization;
using System.Collections;
using View.Extension;
using SysProcessViewModel;
using ViewModelBasic;

namespace SysProcessView
{
    public class CheckStateCvt : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;

            if (isChecked)
            {
                return ToggleState.On;
            }
            else
            {
                return ToggleState.Off;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            ToggleState checkState = (ToggleState)value;

            if (checkState == ToggleState.On || checkState == ToggleState.Indeterminate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class ModuleTreeCvt : IValueConverter
    {
        private RoleSetModuleTreeItem ApplyCheckOnModuleTreeItem(ModuleTreeItem ti)
        {
            var rti = new RoleSetModuleTreeItem
            {
                Icon = ti.Icon,
                IsChecked = false,
                Module = ti.Module
            };
            if (ti.Children != null && ti.Children.Count > 0)
            {
                rti.Children = new List<RoleSetModuleTreeItem>();
                ti.Children.ForEach(c => rti.Children.Add(ApplyCheckOnModuleTreeItem(c)));
            }
            return rti;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<ModuleTreeItem> items = (List<ModuleTreeItem>)value;
            var ritems = items.Select<ModuleTreeItem, RoleSetModuleTreeItem>(ti => ApplyCheckOnModuleTreeItem(ti)).ToList();
            return ritems;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleTreeReadOnlyCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int roleID = (int)value;
            var modules = RoleLogic.ModuleProcessOfRole(roleID);
            return RoleVM.ChangeSysModuleToTreeItem(modules);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 用户ID到用户编号的转换
    /// </summary>
    public class UserIDNameCvt : UserIDNameConvertor
    {
    }

    //public class RolesOfUserCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        UserVM context = (UserVM)parameter;
    //        var roles = RoleVM.RolesOfCurrentOrgnization;
    //        //var users = context.Users;
    //        int userID = (int)value;
    //        var urs = context.UserRoles.FindAll(ur => ur.UserId == userID);
    //        string roleNames = "";
    //        foreach (var ur in urs)
    //        {
    //            var role = roles.FirstOrDefault<SysRole>(r => r.ID == ur.RoleId);
    //            if (role == null) //如果该角色是上级机构创建的
    //            {
    //                role = RoleLogic.GetRole(ur.RoleId);
    //            }
    //            roleNames += (role.Name + ",");
    //        }
    //        return roleNames.TrimEnd(',');
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class BrandsOfUserCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        UserVM context = (UserVM)parameter;
    //        var brands = VMGlobal.PoweredBrands;
    //        int userID = (int)value;
    //        var ubs = context.UserBrands.FindAll(ub => ub.UserID == userID);
    //        string brandNames = "";
    //        foreach (var ub in ubs)
    //        {
    //            var brand = brands.First<ProBrand>(b => b.ID == ub.BrandID);
    //            brandNames += (brand.Name + ",");
    //        }
    //        return brandNames.TrimEnd(',');
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class OrganizationsOfUserCvt : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        UserVM context = (UserVM)parameter;
    //        var orgs = context.Organizations;
    //        int organizationID = (int)value;
    //        if (organizationID == 0)//新增
    //            return "";
    //        return orgs.First(o => o.ID == organizationID).Name;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class RolesForSetCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            UserVM context = parameter as UserVM;
            var roles = context.RolesCurrentUserCover;
            var holdroles = roles.Select(r => new HoldableEntity<SysRole>() { Entity = r, IsHold = false }).ToList();
            IEnumerable<SysRole> myroles = (IEnumerable<SysRole>)value;
            foreach (var hr in holdroles)
            {
                if (myroles.Any(ur => hr.Entity.ID == ur.ID))
                    hr.IsHold = true;
            }
            return holdroles;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 品牌权限
    /// </summary>
    public class BrandsForSetCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            UserVM context = parameter as UserVM;
            //var myubs = context.UserBrands.FindAll(ub => ub.UserID == VMGlobal.CurrentUser.ID);
            var brands = VMGlobal.PoweredBrands;
            var holdbrands = brands.Select(ub => new HoldableEntity<ProBrand> { Entity = ub, IsHold = false }).ToList();
            IEnumerable<ProBrand> mybrands = (IEnumerable<ProBrand>)value;
            foreach (var hb in holdbrands)
            {
                if (mybrands.Any(ub => hb.Entity.ID == ub.ID))
                    hb.IsHold = true;
            }
            return holdbrands;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
