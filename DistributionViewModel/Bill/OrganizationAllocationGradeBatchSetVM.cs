using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using System.ComponentModel;
using Kernel;
using DistributionModel;
using System.Transactions;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class OrganizationAllocationGradeBatchSetVM : CommonViewModel<OrganizationAllocationGradeBO>, IDataErrorInfo
    {
        private string _brandName;

        private int _brandID;
        public int BrandID
        {
            get { return _brandID; }
            set
            {
                _brandID = value;
                var brand = VMGlobal.PoweredBrands.Find(o => o.ID == _brandID);
                if (brand != null)
                {
                    _brandName = brand.Name;
                    Entities = this.SearchData();
                }
                else
                {
                    _brandName = "";
                    Entities = null;
                }
            }
        }

        public OrganizationAllocationGradeBatchSetVM()
        {
            if (VMGlobal.PoweredBrands.Count == 1)
                BrandID = VMGlobal.PoweredBrands[0].ID;
        }

        protected override IEnumerable<OrganizationAllocationGradeBO> SearchData()
        {
            var organizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = organizations.Select(o => o.ID);
            var targets = VMGlobal.DistributionQuery.LinqOP.Search<OrganizationAllocationGrade>(o => oids.Contains(o.OrganizationID) && BrandID == o.BrandID).ToList();
            return organizations.Select(o =>
            {
                var item = new OrganizationAllocationGradeBO
                {
                    BrandName = _brandName,
                    BrandID = BrandID,
                    OrganizationCode = o.Code,
                    OrganizationName = o.Name,
                    OrganizationID = o.ID
                };
                var target = targets.Find(t => t.OrganizationID == o.ID);
                item.Grade = target == null ? 0 : target.Grade;
                item.ID = target == null ? default(int) : target.ID;
                return item;
            }).OrderBy(o => o.OrganizationName).ToList();
        }

        public OPResult Save()
        {
            if (Entities == null || Entities.Count() == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有可供保存的数据." };
            }
            var todeletes = Entities.Where(o => o.Grade == 0 && o.ID != default(int));
            var toau = Entities.Where(o => o.Grade != 0);
            foreach (var au in toau)
            {
                if (au.ID == default(int))
                {
                    au.CreatorID = VMGlobal.CurrentUser.ID;
                    au.CreateTime = DateTime.Now;
                }
            }
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    VMGlobal.DistributionQuery.LinqOP.Delete<OrganizationAllocationGrade>(todeletes);//删除0指标数据
                    VMGlobal.DistributionQuery.LinqOP.AddOrUpdate<OrganizationAllocationGrade>(toau);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "保存成功." };
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n." + e.Message };
                }
            }
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }

            return errorInfo;
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
    }
}
