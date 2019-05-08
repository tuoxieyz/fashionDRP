using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using ERPViewModelBasic;
using Kernel;
using ViewModelBasic;
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class OrganizationGradeForAllocationVM : EditSynchronousVM<OrganizationAllocationGrade>
    {
        public OrganizationGradeForAllocationVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<OrganizationAllocationGrade> SearchData()
        {
            var organizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = organizations.Select(o=>o.ID);
            var brands = VMGlobal.PoweredBrands;
            var bids = brands.Select(o=>o.ID);
            var opfs = LinqOP.Search<OrganizationAllocationGrade>(o=>oids.Contains(o.OrganizationID) && bids.Contains(o.BrandID));
            var result = opfs.Select(o => new OrganizationAllocationGradeBO(o)).ToList();
            return result;
        }

        public override OPResult AddOrUpdate(OrganizationAllocationGrade entity)
        {
            if (!VMGlobal.SysProcessQuery.LinqOP.Any<OrganizationBrand>(o => o.OrganizationID == entity.OrganizationID && o.BrandID == entity.BrandID))
            {
                return new OPResult { IsSucceed = false, Message = "选定的机构和品牌没有对应关系" };
            }
            bool isAdd = entity.ID == default(int);
            if (isAdd)
            {
                if (LinqOP.Any<OrganizationAllocationGrade>(o => o.OrganizationID == entity.OrganizationID && o.BrandID == entity.BrandID))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构指定了对应品牌的等级" };
                }
            }
            else
            {
                if (LinqOP.Any<OrganizationAllocationGrade>(o => o.OrganizationID == entity.OrganizationID && o.ID != entity.ID && o.BrandID == entity.BrandID))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构指定了对应品牌的等级" };
                }
            }
            return base.AddOrUpdate(entity);
        }
    }
}
