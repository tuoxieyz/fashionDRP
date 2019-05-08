using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Kernel;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class VIPKindVM : EditSynchronousVM<VIPKind>
    {
        public VIPKindVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<VIPKind> SearchData()
        {
            return base.SearchData().Select(o => new VIPKindBO(o)).ToList();
        }

        public override OPResult Delete(VIPKind kind)
        {
            if (LinqOP.Any<VIPCardKindMapping>(o => o.KindID == kind.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该VIP类型已使用,不能被删除." };
            }
            return base.Delete(kind);
        }
    }
}
