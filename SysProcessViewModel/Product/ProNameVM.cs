using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kernel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProNameVM : EditSynchronousVM<ProName>
    {
        public ProNameVM():base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.ProNames.Select(o => new ProNameBO(o)).ToList();
        }

        public override OPResult Delete(ProName name)
        {
            if (LinqOP.Any<ProStyle>(o => o.NameID == name.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该品名已被使用，不能被删除，\n若以后不使用，请将状态置为禁用。" };
            }
            var result = base.Delete(name);
            if (result.IsSucceed)
            {
                VMGlobal.ProNames.RemoveAll(o => o.ID == name.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProName entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var name = VMGlobal.ProNames.Find(o => o.ID == entity.ID);
                if (name == null)
                    VMGlobal.ProNames.Add(entity);
                else
                {
                    int index = VMGlobal.ProNames.IndexOf(name);
                    VMGlobal.ProNames[index] = entity;
                }
            }
            return result;
        }
    }
}
