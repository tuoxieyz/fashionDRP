using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using Kernel;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class OuterFactoryVM : EditSynchronousVM<Factory>
    {
        private static List<DataState> _kinds;

        public static List<DataState> Kinds
        {
            get
            {
                if (_kinds == null)
                {
                    _kinds = new List<DataState>();
                    _kinds.Add(new DataState { Flag = true, Name = "外发工厂" });
                    _kinds.Add(new DataState { Flag = false, Name = "内部工厂" });
                }
                return _kinds;
            }
        }

        public OuterFactoryVM()
            : base(VMGlobal.ManufacturingQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<Factory> SearchData()
        {
            return base.SearchData().Select(o => new FactoryBO(o)).ToList();
        }

        public override OPResult Delete(Factory factory)
        {
            if (LinqOP.Any<BillSubcontract>(p => p.OuterFactoryID == factory.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该工厂已经被使用，无法删除。\n若以后不使用，请将状态置为禁用。" };
            }
            return base.Delete(factory);
        }

        public static List<Factory> GetEnabledFactories()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var result = lp.Search<Factory>(o=>o.IsEnabled);
            return result.ToList();
        }
    }
}
