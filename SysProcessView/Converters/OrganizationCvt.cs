using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using DomainLogicEncap;
using System.Windows;
using SysProcessModel;
using SysProcessViewModel;
using ViewModelBasic;

namespace SysProcessView
{
    public class OrganizationTypeCvt : IValueConverter
    {
        private List<SysOrganizationType> _orgTypes;

        /// <summary>
        /// 本级和子级对应的机构类型集合
        /// </summary>
        private List<SysOrganizationType> OrganizationTypes
        {
            get
            {
                if (_orgTypes == null)
                {
                    var organizationContext = OrganizationLogic.Query.LinqOP.Search<SysOrganization>(o => o.ID == VMGlobal.CurrentUser.OrganizationID);
                    var orgTypeContext = OrganizationLogic.Query.LinqOP.GetDataContext<SysOrganizationType>();
                    var data = from ot in orgTypeContext
                               from o in organizationContext
                               where ot.OrganizationID == o.ID || ot.ID == o.TypeId
                               select ot;
                    _orgTypes = data.ToList();
                }
                return _orgTypes;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int tid = (int)value;
            var otype = OrganizationTypes.FirstOrDefault(ot => ot.ID == tid);
            if (otype != null)
                return otype.Name;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrganizationIDCodeCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int oid = (int)value;
            var orgs = OrganizationListVM.CurrentAndChildrenOrganizations;      
            var org = orgs.FirstOrDefault(o => o.ID == oid);
            if (org != null)
                return org.Code;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrganizationIDNameCvtNoContext : IValueConverter
    {
        private List<SysOrganization> _organizationIDNameCache;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_organizationIDNameCache == null)
                _organizationIDNameCache = new List<SysOrganization>();
            int oid = (int)value;
            var organization = _organizationIDNameCache.Find(o => o.ID == oid);
            if (organization == null)
            {
                organization = VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(oid);
                organization = organization ?? new SysOrganization();
                _organizationIDNameCache.Add(organization);
            }
            return organization.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrganizationIDNameCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var orgs = OrganizationListVM.CurrentAndChildrenOrganizations;
            int oid = (int)value;
            var org = orgs.FirstOrDefault(o => o.ID == oid);
            if (org != null)
                return org.Name;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrganizationBrandCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            SysOrganizationBO organization = (SysOrganizationBO)value;
            string brandNames = "";
            foreach (var brand in organization.Brands)
            {
                brandNames += (brand.Name + ",");
            }
            return brandNames.TrimEnd(',');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BrandsForOrganizationSetCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var brands = VMGlobal.PoweredBrands;
            var holdbrands = brands.Select(ob => new HoldableEntity<ProBrand> { Entity = ob, IsHold = false }).ToList();

            SysOrganizationBO organization = (SysOrganizationBO)value;
            if (organization.ID != 0)
            {
                var obs = organization.Brands;
                foreach (var hb in holdbrands)
                {
                    if (obs.Any(ub => hb.Entity.ID == ub.ID))
                        hb.IsHold = true;
                }
            }
            return holdbrands;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 不能修改当前机构本身的品牌权限
    /// </summary>
    public class BrandsCanSetForOrganizationCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int oid = (int)value;
            return VMGlobal.CurrentUser.OrganizationID != oid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BrandIDNameOfOrganizationCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brands = VMGlobal.PoweredBrands;
            int bid = (int)value;
            var brand = brands.FirstOrDefault(b => b.ID == bid);
            if (brand != null)
                return brand.Name;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrganizationTypeEditorStateCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int oid = (int)value;
            bool p = parameter.ToString() == "1";
            if (!((oid == VMGlobal.CurrentUser.OrganizationID) ^ p))//同或,即 !异或
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
