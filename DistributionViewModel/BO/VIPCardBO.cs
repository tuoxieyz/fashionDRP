using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;
using Kernel;
using DBAccess;
using DistributionModel;
using SysProcessViewModel;
using System.Collections.ObjectModel;

namespace DistributionViewModel
{
    public class VIPCardBO : VIPCard, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.DistributionQuery.LinqOP;

        private List<VIPKind> _kinds;
        public List<VIPKind> Kinds
        {
            get
            {
                if (_kinds == null)
                {
                    var cks = _linqOP.Search<VIPCardKindMapping>(o => o.CardID == this.ID);
                    var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
                    var ks = _linqOP.Search<VIPKind>(o => brandIDs.Contains(o.BrandID));
                    var data = from ck in cks
                               from k in ks
                               where ck.KindID == k.ID
                               select k;
                    _kinds = data.ToList();
                }
                return _kinds;
            }
            set
            {
                _kinds = value;
            }
        }

        private ObservableCollection<VIPPointTrack> _pointTracks;
        public ObservableCollection<VIPPointTrack> PointTracks
        {
            get
            {
                if (_pointTracks == null)
                {
                    _pointTracks = new ObservableCollection<VIPPointTrack>(_linqOP.Search<VIPPointTrack>(o => o.VIPID == this.ID).OrderByDescending(o => o.CreateTime).ToList());
                }
                return _pointTracks;
            }
        }

        private ObservableCollection<VIPPredepositTrack> _predeposits;
        public ObservableCollection<VIPPredepositTrack> Predeposits
        {
            get
            {
                if (_predeposits == null)
                {
                    _predeposits = new ObservableCollection<VIPPredepositTrack>(_linqOP.Search<VIPPredepositTrack>(o => o.VIPID == this.ID).OrderByDescending(o => o.CreateTime).ToList());
                }
                return _predeposits;
            }
        }

        /// <summary>
        /// 生日(月日格式，年份为当前年份)
        /// <remarks>报表查询使用</remarks>
        /// </summary>
        public DateTime BirthdayMD
        {
            get;
            //注释的原因参看VIPBrithdayRemindVM
            //{
            //    int year = DateTime.Now.Year;
            //    return new DateTime(year, Birthday.Month, Birthday.Day);
            //}
            set;
        }

        public VIPCardBO()
        { }

        public VIPCardBO(VIPCard vip)
        {
            this.ID = vip.ID;
            this.CustomerName = vip.CustomerName;
            OrganizationID = vip.OrganizationID;
            Address = vip.Address;
            Birthday = vip.Birthday;
            Code = vip.Code;
            IDCard = vip.IDCard;
            //if (OrganizationListVM.CurrentOrganization.ParentID == 0 || vip.OrganizationID == VMGlobal.CurrentUser.OrganizationID)
            //    MobilePhone = vip.MobilePhone;
            //else
            //    MobilePhone = "XXXXXXXXXXX";
            MobilePhone = vip.MobilePhone;
            Sex = vip.Sex;
            CreateTime = vip.CreateTime;
            CreatorID = vip.CreatorID;
            PrestorePassword = vip.PrestorePassword;
        }

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Code")
            {
                if (Code.IsNullEmpty())
                    errorInfo = "不能为空";
                else if (Code.Length < 6)
                    errorInfo = "至少为6位";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<VIPCard>(e => e.Code == Code))
                        errorInfo = "该编号已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<VIPCard>(e => e.ID != ID && e.Code == Code))
                        errorInfo = "该编号已经被使用";
                }
            }
            else if (columnName == "OrganizationID")
            {
                if (OrganizationID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "CustomerName")
            {
                if (CustomerName.IsNullEmpty())
                    errorInfo = "不能为空";
            }
            else if (columnName == "MobilePhone")
            {
                if (!MobilePhone.IsNullEmpty())
                {
                    if (!MobilePhone.IsMobile())
                        errorInfo = "格式不正确";
                    else if (ID == 0)//新增
                    {
                        if (_linqOP.Any<VIPCard>(e => e.MobilePhone == MobilePhone))
                            errorInfo = "该手机号码已经被使用";
                    }
                    else//编辑
                    {
                        if (_linqOP.Any<VIPCard>(e => e.ID != ID && e.MobilePhone == MobilePhone))
                            errorInfo = "该手机号码已经被使用";
                    }
                }
            }

            return errorInfo;
        }


        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return this.CheckData(columnName);
            }
        }
    }
}
