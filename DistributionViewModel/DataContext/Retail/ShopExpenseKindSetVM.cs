using DistributionModel;
using Kernel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class ShopExpenseKindSetVM : EditSynchronousVM<ShopExpenseKind>
    {
        public ShopExpenseKindSetVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = LinqOP.Search<ShopExpenseKind>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).Select(o => new ShopExpenseKindBO(o)).ToList();
        }

        public override OPResult Delete(ShopExpenseKind entity)
        {
            if (LinqOP.Any<ShopExpense>(o => o.ExpenseKindID == entity.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该费用类别已经被使用，\n若以后不再使用，请将状态置为禁用。" };
            }
            return base.Delete(entity);
        }
    }
}
