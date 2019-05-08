using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kernel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProUnitVM: EditSynchronousVM<ProUnit>
    {
        public ProUnitVM():base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.Units.Select(o => new ProUnitBO(o)).ToList();
        }

        public override OPResult Delete(ProUnit unit)
        {
            if (LinqOP.Any<ProStyle>(o => o.UnitID == unit.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该单位已被使用，不能被删除，\n若以后不使用，请将状态置为禁用。" };
            }
            var result = base.Delete(unit);
            if (result.IsSucceed)
            {
                VMGlobal.Units.RemoveAll(o => o.ID == unit.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProUnit entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var unit = VMGlobal.Units.Find(o => o.ID == entity.ID);
                if (unit == null)
                    VMGlobal.Units.Add(entity);
                else
                {
                    int index = VMGlobal.Units.IndexOf(unit);
                    VMGlobal.Units[index] = entity;
                }
            }
            return result;
        }
    }
}
