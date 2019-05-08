using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;
using ManufacturingModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class BillProductExchangeManageVM : BillProductExchangeSearchVM
    {
        /// <summary>
        /// 逻辑删除
        /// </summary>
        public OPResult DeleteBill(BillProductExchangeSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var bill = lp.GetById<BillProductExchange>(entity.ID);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.IsDeleted = true;
            try
            {
                lp.Update<BillProductExchange>(bill);
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
        public OPResult RevertBill(BillProductExchangeSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var bill = lp.GetById<BillProductExchange>(entity.ID);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.IsDeleted = false;
            try
            {
                lp.Update<BillProductExchange>(bill);
                return new OPResult { IsSucceed = true, Message = "恢复成功!" };
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "恢复失败,失败原因:\n" + ex.Message };
            }
        }

        public OPResult ReSendBill(BillProductExchangeSearchEntity entity)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var bill = lp.GetById<BillProductExchange>(entity.ID);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.Status = (int)BillProductExchangeStatusEnum.在途中;
            bill.Remark = entity.Remark;
            try
            {
                lp.Update<BillProductExchange>(bill);
                return new OPResult { IsSucceed = true, Message = "重新发送成功!" };
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "重新发送失败,失败原因:\n" + ex.Message };
            }
        }

        public OPResult UpdateDetails(ProductForProductExchange pe)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var details = lp.GetById<BillProductExchangeDetails>(pe.ID);
            details.Quantity = pe.Quantity;
            try
            {
                lp.Update<BillProductExchangeDetails>(details);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "更新失败,失败原因:" + e.Message };
            }
            return new OPResult { IsSucceed = true, Message = "更新成功." };
        }

        public void SetQuantityForBillEntity(BillProductExchangeSearchEntity entity)
        {
            entity.Quantity = entity.Details.Sum(o => o.Quantity);
        }
    }
}
