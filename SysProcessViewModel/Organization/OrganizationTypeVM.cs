using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;
using Kernel;
using ViewModelBasic;

namespace SysProcessViewModel
{
    public class OrganizationTypeVM: EditSynchronousVM<SysOrganizationType>
    {
        public OrganizationTypeVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<SysOrganizationType> SearchData()
        {
            var result = LinqOP.Search<SysOrganizationType>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            return result.Select(o => new SysOrganizationTypeBO(o)).ToList();
        }

        public override OPResult Delete(SysOrganizationType type)
        {
            if (LinqOP.Any<SysOrganization>(o => o.ParentID == VMGlobal.CurrentUser.OrganizationID && o.TypeId == type.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该类型已被使用，不能被删除，\n若以后不使用，请将状态置为禁用。" };
            }
            var result = base.Delete(type);
            if (result.IsSucceed)
            {
                VMGlobal.OrganizationTypes.RemoveAll(o => o.ID == type.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(SysOrganizationType entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var type = VMGlobal.OrganizationTypes.Find(o => o.ID == entity.ID);
                if (type == null)
                    VMGlobal.OrganizationTypes.Add(entity);
                else
                {
                    int index = VMGlobal.OrganizationTypes.IndexOf(type);
                    VMGlobal.OrganizationTypes[index] = entity;
                }
                if (!entity.IsEnabled)
                    VMGlobal.OrganizationTypes.Remove(entity);
            }
            return result;
        }
    }
}
