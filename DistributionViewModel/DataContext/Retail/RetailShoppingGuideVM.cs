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
    public class RetailShoppingGuideVM : EditSynchronousVM<RetailShoppingGuide>
    {
        public RetailShoppingGuideVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = LinqOP.Search<RetailShoppingGuide>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).Select(o => new RetailShoppingGuideBO(o)).ToList();
        }

        public override OPResult Delete(RetailShoppingGuide guide)
        {
            if (LinqOP.Any<BillRetail>(o => o.GuideID == guide.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该导购已经有销售记录，不可删除，\n若以后不再使用，请将状态置为禁用。" };
            }
            return base.Delete(guide);
        }
    }
}
