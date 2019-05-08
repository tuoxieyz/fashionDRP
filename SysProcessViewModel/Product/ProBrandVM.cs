using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kernel;
using System.Transactions;
using ViewModelBasic;

using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProBrandVM : EditSynchronousVM<ProBrand>
    {
        public ProBrandVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.PoweredBrands.Select(o => new ProBrandBO(o)).ToList();
        }

        public override OPResult AddOrUpdate(ProBrand entity)
        {
            int id = entity.ID;
            using (TransactionScope scope = new TransactionScope())
            {
                var result = base.AddOrUpdate(entity);
                if (result.IsSucceed)
                {
                    if (id == default(int))
                    {
                        var ub = new UserBrand { UserID = VMGlobal.CurrentUser.ID, BrandID = entity.ID, CreateTime = DateTime.Now, CreatorID = VMGlobal.CurrentUser.ID };
                        var ob = new OrganizationBrand { OrganizationID = VMGlobal.CurrentUser.OrganizationID, BrandID = entity.ID, CreateTime = DateTime.Now, CreatorID = VMGlobal.CurrentUser.ID };
                        LinqOP.Add<UserBrand>(ub);
                        LinqOP.Add<OrganizationBrand>(ob);
                    }
                    scope.Complete();

                    var brand = VMGlobal.PoweredBrands.Find(o => o.ID == entity.ID);
                    if (brand == null)
                        VMGlobal.PoweredBrands.Add(entity);
                    else
                    {
                        int index = VMGlobal.PoweredBrands.IndexOf(brand);
                        VMGlobal.PoweredBrands[index] = entity;
                    }
                }
                return result;
            }
        }
    }
}
