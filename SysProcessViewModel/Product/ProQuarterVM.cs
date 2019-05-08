using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kernel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProQuarterVM: EditSynchronousVM<ProQuarter>
    {
        public ProQuarterVM():base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.Quarters.Select(o => new ProQuarterBO(o)).ToList();
        }

        public override OPResult Delete(ProQuarter quarter)
        {
            if (LinqOP.Any<ProBYQ>(o => o.Quarter == quarter.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该季度已被使用，不能被删除，\n若以后不使用，请将状态置为禁用。" };
            }
            var result = base.Delete(quarter);
            if (result.IsSucceed)
            {
                VMGlobal.Quarters.RemoveAll(o => o.ID == quarter.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProQuarter entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var quarter = VMGlobal.Quarters.Find(o => o.ID == entity.ID);
                if (quarter == null)
                    VMGlobal.Quarters.Add(entity);
                else
                {
                    int index = VMGlobal.Quarters.IndexOf(quarter);
                    VMGlobal.Quarters[index] = entity;
                }
            }
            return result;
        }
    }
}
