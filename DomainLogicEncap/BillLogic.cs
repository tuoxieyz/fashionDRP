//*********************************************
// 公司名称：              
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-5-25 15:50:24
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using Kernel;
using Model.Extension;
using DistributionModel;

namespace DomainLogicEncap
{
    public static class BillLogic
    {
        private static QueryGlobal _query = new QueryGlobal("DistributionConstr");

        public static LinqOPEncap LinqOP
        {
            get { return _query.LinqOP; }
        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public static OPResult DeleteBill<T>(int bid) where T : IsDeletedEntity
        {
            var bill = _query.LinqOP.GetById<T>(bid);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.IsDeleted = true;
            try
            {
                _query.LinqOP.Update<T>(bill);
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
        public static OPResult RevertBill<T>(int bid) where T : IsDeletedEntity
        {
            var bill = _query.LinqOP.GetById<T>(bid);
            if (bill == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            bill.IsDeleted = false;
            try
            {
                _query.LinqOP.Update<T>(bill);
                return new OPResult { IsSucceed = true, Message = "恢复成功!" };
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "恢复失败,失败原因:\n" + ex.Message };
            }
        }

        /// <summary>
        /// 增加库存
        /// </summary>
        /// <param name="storageID">仓库ID</param>
        /// <param name="productID">条码ID</param>
        /// <param name="quantity">增加的数量</param>
        public static void AddStock(int storageID, int productID, int quantity)
        {
            var stock = _query.LinqOP.Search<Stock>(o => o.StorageID == storageID && o.ProductID == productID).FirstOrDefault();
            if (stock != null)
                stock.Quantity += quantity;
            else
                stock = new Stock { StorageID = storageID, ProductID = productID, Quantity = quantity };
            _query.LinqOP.AddOrUpdate<Stock>(stock);
        }

        #region 订单相关     

        /// <summary>
        /// 获取剩余订单量
        /// </summary>
        public static int GetRemainOrderQuantity(int organizationID, int productID)
        {
            var orders = _query.LinqOP.GetDataContext<BillOrder>();
            var orderDetails = _query.LinqOP.GetDataContext<BillOrderDetails>();
            var query = from o in orders
                        from od in orderDetails
                        where o.ID == od.BillID && o.OrganizationID == organizationID && !o.IsDeleted && od.ProductID == productID && (od.Quantity > od.QuaCancel + od.QuaDelivered)
                        select od;
            return query.Sum(o => (o.Quantity - o.QuaCancel - o.QuaDelivered));
        }

        #endregion

        /// <summary>
        /// 发货时更新订单(在发货量大时不知会否有效率问题)
        /// </summary>
        public static void UpdateOrderWhenDelivery(int organizationID, int productID, int quantity)
        {
            _query.DB.ExecuteNonQuery("UpdateOrderWhenDelivery", organizationID, productID, quantity);
        }

        public static void UpdateOrderWhenCancelDelivery(int organizationID, int productID, int quantity)
        {
            _query.DB.ExecuteNonQuery("UpdateOrderWhenCancelDelivery", organizationID, productID, quantity);
        }
    }
}
