using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;
using SysProcessModel;
using ViewModelBasic;
using Kernel;

using DBAccess;

using DomainLogicEncap;

namespace SysProcessViewModel
{
    public class SysOrganizationBO : SysOrganization, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.SysProcessQuery.LinqOP;
        private DataChecker _checker;

        private List<ProBrand> _brands;
        public List<ProBrand> Brands
        {
            get
            {
                if (_brands == null)
                {
                    var brandIDs = _linqOP.Search<OrganizationBrand>(o => o.OrganizationID == this.ID).Select(o => o.BrandID).ToArray();
                    _brands = VMGlobal.PoweredBrands.FindAll(o => brandIDs.Contains(o.ID)).ToList();
                }
                return _brands;
            }
            set
            {
                _brands = value;
            }
        }

        private List<SysOrganizationBO> _childrenOrganizations;
        public List<SysOrganizationBO> ChildrenOrganizations
        {
            get
            {
                if (_childrenOrganizations == null)
                    _childrenOrganizations = OrganizationLogic.GetChildOrganizations(this.ID).Select(o => new SysOrganizationBO(o)).ToList();
                return _childrenOrganizations;
            }
        }

        public SysOrganizationBO()
        { }

        public SysOrganizationBO(SysOrganization organization)
        {
            this.ID = organization.ID;
            this.Name = organization.Name;
            this.Code = organization.Code;
            this.ParentID = organization.ParentID;
            this.TypeId = organization.TypeId;
            this.Flag = organization.Flag;
            this.AreaID = organization.AreaID;
            this.ProvienceID = organization.ProvienceID;
            this.CityID = organization.CityID;
            this.Address = organization.Address;
            this.Telephone = organization.Telephone;
            this.Linkman = organization.Linkman;
            this.Description = organization.Description;
            CreateTime = organization.CreateTime;
            CreatorID = organization.CreatorID;
            Latitude = organization.Latitude;
            Longitude = organization.Longitude;
            MapCityName = organization.MapCityName;
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
                errorInfo = _checker.CheckDataCodeName<SysOrganization>(this, columnName);
            }
            else if (columnName == "Telephone")
            {
                if (!Telephone.IsNullEmpty() && !(Telephone.IsTelepone() || Telephone.IsMobile()))
                    errorInfo = "格式不正确";
            }
            else if (columnName == "TypeId")
            {
                if (TypeId == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "AreaID")
            {
                if (AreaID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "ProvienceID")
            {
                if (ProvienceID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "CityID")
            {
                if (CityID == default(int))
                    errorInfo = "不能为空";
            }
            return errorInfo;
        }
    }
}
