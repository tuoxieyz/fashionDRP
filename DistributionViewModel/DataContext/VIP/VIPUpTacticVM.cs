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
    public class VIPUpTacticVM : EditSynchronousVM<VIPUpTactic>
    {
        public VIPUpTacticVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = base.SearchData().Select(o => new VIPUpTacticValid(o)).ToList();
        }

        public override OPResult AddOrUpdate(VIPUpTactic kind)
        {
            if (kind.OnceConsume == 0 && (kind.DateSpan == 0 || kind.SpanConsume == 0))
            {
                return new OPResult { IsSucceed = false, Message = "单次消费和累计消费至少设置其中一个.\n即单次消费金额不能设为0,或者累计消费时间和消费金额不能设为0." };
            }
            if (kind.ID == default(int))
            {
                if (LinqOP.Any<VIPUpTactic>(o => o.BrandID == kind.BrandID && o.FormerKindID == kind.FormerKindID && o.AfterKindID == kind.AfterKindID && o.IsEnabled))
                {
                    return new OPResult { IsSucceed = false, Message = "已经设置了相应VIP卡类型升级策略" };
                }
            }
            else
            {
                if (LinqOP.Any<VIPUpTactic>(o => o.BrandID == kind.BrandID && o.FormerKindID == kind.FormerKindID && o.AfterKindID == kind.AfterKindID && o.IsEnabled && o.ID != kind.ID && kind.IsEnabled))
                {
                    return new OPResult { IsSucceed = false, Message = "已经设置了相应VIP卡类型升级策略" };
                }
            }
            return base.AddOrUpdate(kind);
        }
    }
}
