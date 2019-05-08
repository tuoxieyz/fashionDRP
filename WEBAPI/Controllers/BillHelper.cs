using DistributionModel;
using ERPModelBO;
using Kernel;
using Model.Extension;
using SysProcessModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace WEBAPI.Controllers
{
    internal static class BillHelper
    {
        internal static string GenerateBillCode<T>(int organizationID, DbContext dbContext, Expression<Func<T, bool>> funcOrganizationID = null) where T : BillBase
        {
            DateTime time = DateTime.Now;
            //DateTime nexttime = time.AddDays(1);
            string timestr = "-" + time.ToString("yyyyMMdd");
            if (funcOrganizationID == null)
                funcOrganizationID = o => o.OrganizationID == organizationID;
            //o.CreateTime >= time.Date && o.CreateTime <= nexttime.Date || 
            var maxCode = dbContext.Set<T>().Where(funcOrganizationID).Where(o => o.Code.Contains(timestr)).Max(t => t.Code);
            if (string.IsNullOrEmpty(maxCode))
            {
                int tag = (int)Enum.Parse(typeof(BillTypeEnum), typeof(T).Name);
                string prefixion = Enum.GetName(typeof(BillCodePrefixion), tag);
                var ocode = dbContext.Set<ViewOrganization>().Where(b => b.ID == organizationID).Select(o => o.Code).First();
                maxCode = prefixion + ocode + timestr + "000";
            }
            int preLength = maxCode.Length - 3;
            return maxCode.Substring(0, preLength) + (Convert.ToInt32(maxCode.Substring(preLength)) + 1).ToString("000");
        }

        /// <summary>
        /// 保存单据
        /// </summary>
        public static OPResult SaveBill<T, TDetail, TDbContext>(BillBO<T, TDetail> bo)
            where T : BillBase
            where TDetail : BillDetailBase
            where TDbContext : DbContext, new()
        {
            try
            {
                using (var dbContext = new TDbContext())
                {
                    BillHelper.SaveBill<T, TDetail>(bo, dbContext);
                }
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + ex.Message };
            }
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }

        /// <summary>
        /// 保存单据
        /// </summary>
        public static void SaveBill<T, TDetail>(BillBO<T, TDetail> bo, DbContext dbContext, bool specifcCreateTime = false)
            where T : BillBase
            where TDetail : BillDetailBase
        {
            if (!specifcCreateTime)
                bo.Bill.CreateTime = DateTime.Now;

            var tcontext = dbContext.Set<T>();

            if (string.IsNullOrEmpty(bo.Bill.Code))
                bo.Bill.Code = BillHelper.GenerateBillCode<T>(bo.Bill.OrganizationID, dbContext);

            tcontext.Add(bo.Bill);
            dbContext.SaveChanges();

            var dcontext = dbContext.Set<TDetail>();
            foreach (var d in bo.Details)
            {
                d.BillID = bo.Bill.ID;
                dcontext.Add(d);
            }
            dbContext.SaveChanges();
        }

        /// <summary>
        /// 生成出库单
        /// </summary>
        public static BillBO<BillStoreOut, BillStoreOutDetails> GenerateStoreOut<T, TDetail>(BillBO<T, TDetail> bo)
            where T : BillWithBrand, new()
            where TDetail : BillDetailBase
        {
            BillBO<BillStoreOut, BillStoreOutDetails> storeout = new BillBO<BillStoreOut, BillStoreOutDetails>();
            var bill = storeout.Bill = new BillStoreOut();
            int typeid = (int)Enum.Parse(typeof(BillTypeEnum), typeof(T).Name);
            bill.Remark = Enum.GetName(typeof(BillStoreOutTypeEnum), typeid);
            bill.BillType = typeid;
            bill.OrganizationID = bo.Bill.OrganizationID;
            bill.BrandID = bo.Bill.BrandID;
            bill.RefrenceBillCode = bo.Bill.Code;
            bill.CreatorID = bo.Bill.CreatorID;
            if (bo.Bill is IStorageID)
                bill.StorageID = ((IStorageID)bo.Bill).StorageID;

            storeout.Details = new List<BillStoreOutDetails>();
            foreach (var p in bo.Details)
            {
                storeout.Details.Add(new BillStoreOutDetails
                {
                    ProductID = p.ProductID,
                    Quantity = p.Quantity
                });
            };
            return storeout;
        }

        internal static void SaveBillStoreOut(BillBO<BillStoreOut, BillStoreOutDetails> bo, DbContext dbContext)
        {
            BillHelper.SaveBill<BillStoreOut, BillStoreOutDetails>(bo, dbContext);
            dbContext.Database.ExecuteSqlCommand("exec UpdateStockWhenStoreIO {0}, {1}", bo.Bill.ID, false);
        }

        /// <summary>
        /// 生成入库单
        /// </summary>
        public static BillBO<BillStoring, BillStoringDetails> GenerateStoring<T, TDetail>(BillBO<T, TDetail> bo)
            where T : BillWithBrand, new()
            where TDetail : BillDetailBase
        {
            BillBO<BillStoring, BillStoringDetails> storing = new BillBO<BillStoring, BillStoringDetails>();
            var bill = storing.Bill = new BillStoring();
            int typeid = (int)Enum.Parse(typeof(BillTypeEnum), typeof(T).Name);
            bill.Remark = Enum.GetName(typeof(BillStoringTypeEnum), typeid);
            bill.BillType = typeid;
            bill.OrganizationID = bo.Bill.OrganizationID;
            bill.BrandID = bo.Bill.BrandID;
            bill.RefrenceBillCode = bo.Bill.Code;
            bill.CreatorID = bo.Bill.CreatorID;
            if (bo.Bill is IStorageID)
                bill.StorageID = ((IStorageID)bo.Bill).StorageID;

            storing.Details = new List<BillStoringDetails>();
            foreach (var p in bo.Details)
            {
                storing.Details.Add(new BillStoringDetails
                {
                    ProductID = p.ProductID,
                    Quantity = p.Quantity
                });
            };
            return storing;
        }

        internal static void SaveBillStoring(BillBO<BillStoring, BillStoringDetails> bo, DbContext dbContext)
        {
            BillHelper.SaveBill<BillStoring, BillStoringDetails>(bo, dbContext);
            dbContext.Database.ExecuteSqlCommand("exec UpdateStockWhenStoreIO {0}, {1}", bo.Bill.ID, true);
        }
    }
}