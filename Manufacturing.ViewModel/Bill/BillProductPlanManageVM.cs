using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using SysProcessViewModel;
using Kernel;

namespace Manufacturing.ViewModel
{
    public class BillProductPlanManageVM : BillProductPlanSearchVM
    {
        public OPResult UpdateDetails(ProductForProduceBrush plan)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var details = lp.GetById<BillProductPlanDetails>(plan.ID);
            details.QuaCancel = plan.QuaCancel;
            details.QuaCompleted = plan.QuaCompleted;
            details.DeliveryDate = plan.DeliveryDate;
            try
            {
                lp.Update<BillProductPlanDetails>(details);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "更新失败,失败原因:" + e.Message };
            }
            return new OPResult { IsSucceed = true, Message = "更新成功." };
        }

        public void SetQuantityForBillEntity(BillProductPlanSearchEntity entity)
        {
            entity.Quantity = entity.Details.Sum(o => o.Quantity);
            entity.QuaCancel = entity.Details.Sum(o => o.QuaCancel);
            entity.QuaCompleted = entity.Details.Sum(o => o.QuaCompleted);
            var realSubcontractQuantity = entity.Quantity - entity.QuaCancel;
            entity.StatusName = realSubcontractQuantity == entity.QuaCompleted ? "已完成" : (entity.QuaCompleted == 0 ? "未交货" : (realSubcontractQuantity > entity.QuaCompleted ? "部分已交货" : "数据有误"));
        }

        /// <summary>
        /// 取消剩余
        /// </summary>
        public OPResult CancelLeftSubcontractQuantity(BillProductPlanSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var orders = lp.Search<BillProductPlanDetails>(o => o.BillID == entity.ID).ToList();
            orders.ForEach(o =>
            {
                o.QuaCancel = o.Quantity - o.QuaCompleted;
            });
            try
            {
                lp.Update<BillProductPlanDetails>(orders);
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "取消失败,失败原因:\n" + ex.Message };
            }
            foreach (var d in entity.Details)
            {
                d.QuaCancel = d.Quantity - d.QuaCompleted;
            }
            return new OPResult { IsSucceed = true, Message = "取消成功!" };
        }

        /// <summary>
        /// 取消量归零
        /// </summary>
        public OPResult ZeroCancelSubcontractQuantity(BillProductPlanSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var orders = lp.Search<BillProductPlanDetails>(o => o.BillID == entity.ID).ToList();
            orders.ForEach(o =>
            {
                o.QuaCancel = 0;
            });
            try
            {
                lp.Update<BillProductPlanDetails>(orders);
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "取消量归零失败,失败原因:\n" + ex.Message };
            }
            foreach (var d in entity.Details)
            {
                d.QuaCancel = 0;
            }
            return new OPResult { IsSucceed = true, Message = "取消量归零成功!" };
        }
    }
}
