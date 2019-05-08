using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Kernel;
using DistributionModel.Finance;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class VoucherItemKindVM : EditSynchronousVM<VoucherItemKind>
    {
        private int _kind;

        public VoucherItemKindVM(int kind)
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            _kind = kind;
            Entities = LinqOP.Search<VoucherItemKind>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Kind == kind).Select(o => new VoucherItemKindBO(o)).ToList();
        }

        public override OPResult Delete(VoucherItemKind entity)
        {
            if (_kind == 1)
            {
                if (LinqOP.Any<VoucherDeductMoney>(o => o.ItemKindID == entity.ID))
                {
                    return new OPResult { IsSucceed = false, Message = "该扣款项目已经被使用，\n若以后不再使用，请将状态置为禁用。" };
                }
            }
            else if (_kind == 2)
            {
                if (LinqOP.Any<VoucherReceiveMoney>(o => o.ReceiveKindID == entity.ID))
                {
                    return new OPResult { IsSucceed = false, Message = "该收款项目已经被使用，\n若以后不再使用，请将状态置为禁用。" };
                }
            }
            return base.Delete(entity);
        }
    }
}
