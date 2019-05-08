using DistributionModel;
using ERPModelBO;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class RetailForSubordinateVM : BaseBillRetailVM
    {
        private int _organizationID;

        public int OrganizationID
        {
            get { return _organizationID; }
            set
            {
                _organizationID = value;
                if (value == default(int))
                {
                    _shifts = null;
                    Storages = null;
                    _guides = null;
                    Users = null;
                }
                else
                {
                    var lp = VMGlobal.DistributionQuery.LinqOP;
                    _shifts = lp.Search<RetailShift>(o => o.OrganizationID == value && o.IsEnabled).ToList();
                    Storages = lp.Search<Storage>(o => o.OrganizationID == value && o.Flag).ToList();
                    _guides = lp.Search<RetailShoppingGuide>(o => o.OrganizationID == value && o.State && o.OnBoardDate <= DateTime.Now && (o.DimissionDate == null || o.DimissionDate > DateTime.Now.Date)).ToList();
                    Users = VMGlobal.SysProcessQuery.LinqOP.Search<SysUser>(o => o.OrganizationID == value && o.Flag).ToList();
                }
                OnPropertyChanged("OrganizationID");
                OnPropertyChanged("Shifts");
                OnPropertyChanged("Storages");
                OnPropertyChanged("Guides");
                OnPropertyChanged("Users");
                OnPropertyChanged("BirthdayTactic");
            }
        }

        private List<RetailShift> _shifts = null;

        public override List<RetailShift> Shifts
        {
            get
            {
                return _shifts;
            }
        }

        public IEnumerable<Storage> Storages { get; set; }

        private List<RetailShoppingGuide> _guides = null;

        public IEnumerable<RetailShoppingGuide> Guides
        {
            get
            {
                //if (_guides != null && Master.ShiftID != default(int))
                //{
                //    return _guides.Where(o => o.ShiftID == Master.ShiftID);
                //}
                return _guides;
            }
        }

        public IEnumerable<SysUser> Users { get; set; }

        private VIPBirthdayTactic GetVIPBirthdayTactic()
        {
            if (OrganizationID == default(int))
                return null;
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var organizations = lp.Search<ViewOrganization>(o => o.ID == OrganizationID);
            var tactics = lp.GetDataContext<VIPBirthdayTactic>();
            tactics = from tactic in tactics
                      from organization in organizations
                      where tactic.OrganizationID == organization.ID || tactic.OrganizationID == organization.ParentID
                      select tactic;
            return tactics.OrderByDescending(o => o.OrganizationID).FirstOrDefault();
        }

        public override OPResult ValidateWhenCash()
        {
            if (this.OrganizationID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择零售店铺" };
            }
            return base.ValidateWhenCash();
        }

        public override void SetRetailData()
        {
            int userid = Master.CreatorID;
            base.SetRetailData();
            if (userid != default(int))
                Master.CreatorID = userid;
            this.Master.OrganizationID = OrganizationID;
        }

        protected override BillRetailBO GenerateRetailBO()
        {
            var bo = base.GenerateRetailBO();
            bo.SpecifcCreateTime = true;
            return bo;
        }

        public override void Init()
        {
            var createTime = this.Master.CreateTime;
            var storageID = this.Master.StorageID;
            var shiftID = this.Master.ShiftID;
            var creatorID = this.Master.CreatorID;
            var guideID = this.Master.GuideID;
            base.Init();
            this.Master = new BillRetail
            {
                CreateTime = createTime,
                StorageID = storageID,
                ShiftID = shiftID,
                CreatorID = creatorID,
                GuideID = guideID
            };
        }
    }
}
