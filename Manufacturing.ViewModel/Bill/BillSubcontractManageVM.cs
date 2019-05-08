using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using Kernel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class BillSubcontractManageVM : BillSubcontractSearchVM
    {
        public OPResult UpdateDetails(ProductForProduceBrush subcontract)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var details = lp.GetById<BillSubcontractDetails>(subcontract.ID);
            details.QuaCancel = subcontract.QuaCancel;
            details.QuaCompleted = subcontract.QuaCompleted;
            details.DeliveryDate = subcontract.DeliveryDate;
            try
            {
                lp.Update<BillSubcontractDetails>(details);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "更新失败,失败原因:" + e.Message };
            }
            return new OPResult { IsSucceed = true, Message = "更新成功." };
        }

        public void SetQuantityForBillEntity(BillSubcontractSearchEntity entity)
        {
            entity.Quantity = entity.Details.Sum(o => o.Quantity);
            entity.QuaCancel = entity.Details.Sum(o => o.QuaCancel);
            entity.QuaCompleted = entity.Details.Sum(o => o.QuaCompleted);
            var realSubcontractQuantity = entity.Quantity - entity.QuaCancel;
            entity.StatusName = realSubcontractQuantity == entity.QuaCompleted ? "已完成" : (entity.QuaCompleted == 0 ? "未交货" : (realSubcontractQuantity > entity.QuaCompleted ? "部分已交货" : "数据有误"));
        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public OPResult DeleteBill(BillSubcontractSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var bill = lp.GetById<BillSubcontract>(entity.ID);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.IsDeleted = true;
            try
            {
                lp.Update<BillSubcontract>(bill);
                return new OPResult { IsSucceed = true, Message = "作废成功!" };
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "作废失败,失败原因:\n" + ex.Message };
            }
        }

        /// <summary>
        /// 恢复单据
        /// </summary>
        public OPResult RevertBill(BillSubcontractSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var bill = lp.GetById<BillSubcontract>(entity.ID);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.IsDeleted = false;
            try
            {
                lp.Update<BillSubcontract>(bill);
                return new OPResult { IsSucceed = true, Message = "恢复成功!" };
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "恢复失败,失败原因:\n" + ex.Message };
            }
        }

        /// <summary>
        /// 取消剩余
        /// </summary>
        public OPResult CancelLeftSubcontractQuantity(BillSubcontractSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var orders = lp.Search<BillSubcontractDetails>(o => o.BillID == entity.ID).ToList();
            orders.ForEach(o =>
            {
                o.QuaCancel = o.Quantity - o.QuaCompleted;
            });
            try
            {
                lp.Update<BillSubcontractDetails>(orders);                
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
        public OPResult ZeroCancelSubcontractQuantity(BillSubcontractSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var orders = lp.Search<BillSubcontractDetails>(o => o.BillID == entity.ID).ToList();
            orders.ForEach(o =>
            {
                o.QuaCancel = 0;
            });
            try
            {
                lp.Update<BillSubcontractDetails>(orders);                
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
