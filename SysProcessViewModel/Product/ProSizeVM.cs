using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kernel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProSizeVM: EditSynchronousVM<ProSize>
    {
        public ProSizeVM():base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.Sizes.Select(o => new ProSizeBO(o)).ToList();
        }
        
        public override OPResult Delete(ProSize size)
        {
            if (LinqOP.Any<Product>(o => o.SizeID == size.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该尺码已被使用，不能被删除，\n若以后不使用，请将状态置为禁用。" };
            }
            var result = base.Delete(size);
            if (result.IsSucceed)
            {
                VMGlobal.Sizes.RemoveAll(o => o.ID == size.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProSize entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var size = VMGlobal.Sizes.Find(o => o.ID == entity.ID);
                if (size == null)
                    VMGlobal.Sizes.Add(entity);
                else
                {
                    int index = VMGlobal.Sizes.IndexOf(size);
                    VMGlobal.Sizes[index] = entity;
                }
            }
            return result;
        }
    }
}
