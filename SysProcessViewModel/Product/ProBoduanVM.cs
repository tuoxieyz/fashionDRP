using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kernel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProBoduanVM: EditSynchronousVM<ProBoduan>
    {
        public ProBoduanVM():base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.Boduans.Select(o => new ProBoduanBO(o)).ToList();
        }

        public override OPResult Delete(ProBoduan boduan)
        {
            if (LinqOP.Any<ProStyle>(o => o.BoduanID == boduan.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该波段已被使用，不能被删除，\n若以后不使用，请将状态置为禁用。" };
            }
            var result = base.Delete(boduan);
            if (result.IsSucceed)
            {
                VMGlobal.Boduans.RemoveAll(o => o.ID == boduan.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProBoduan entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var boduan = VMGlobal.Boduans.Find(o => o.ID == entity.ID);
                if (boduan == null)
                    VMGlobal.Boduans.Add(entity);
                else
                {
                    int index = VMGlobal.Boduans.IndexOf(boduan);
                    VMGlobal.Boduans[index] = entity;
                }
            }
            return result;
        }
    }
}
