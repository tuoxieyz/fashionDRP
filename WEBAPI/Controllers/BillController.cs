using DBEncapsulation;
using DistributionModel;
using ERPModelBO;
using Kernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace WEBAPI.Controllers
{
    public class BillController : ApiController
    {
        public OPResult SaveBillStoreMove(BillBO<BillStoreMove, BillStoreMoveDetails> bo)
        {
            using (var dbContext = new DistributionEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        BillHelper.SaveBill<BillStoreMove, BillStoreMoveDetails>(bo, dbContext);
                        var storeout = BillHelper.GenerateStoreOut(bo);
                        storeout.Bill.StorageID = bo.Bill.StorageIDOut;
                        BillHelper.SaveBillStoreOut(storeout, dbContext);
                        var storing = BillHelper.GenerateStoring(bo);
                        storing.Bill.StorageID = bo.Bill.StorageIDIn;
                        BillHelper.SaveBillStoring(storing, dbContext);

                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }

        public OPResult SaveBillStoring(BillBO<BillStoring, BillStoringDetails> bo)
        {
            using (var dbContext = new DistributionEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        BillHelper.SaveBillStoring(bo, dbContext);
                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }

        public OPResult SaveBillAllocate(BillBO<BillAllocate, BillAllocateDetails> bo)
        {
            return BillHelper.SaveBill<BillAllocate, BillAllocateDetails, DistributionEntities>(bo);
        }

        public OPResult SaveBillDelivery(BillDeliveryBO bo)
        {
            using (var dbContext = new DistributionEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        bo.Bill.Code = BillHelper.GenerateBillCode<BillDelivery>(bo.Bill.ToOrganizationID, dbContext, o => o.ToOrganizationID == bo.Bill.ToOrganizationID);
                        BillHelper.SaveBill<BillDelivery, BillDeliveryDetails>(bo, dbContext);
                        var storeout = BillHelper.GenerateStoreOut(bo);
                        BillHelper.SaveBillStoreOut(storeout, dbContext);
                        if (bo.FundAccount != null && bo.FundAccount.NeedIn != 0)
                        {
                            bo.FundAccount.RefrenceBillCode = bo.Bill.Code;
                            dbContext.OrganizationFundAccount.Add(bo.FundAccount);
                        }
                        if (bo.Bill.IsWriteDownOrder)
                        {
                            //冲减订单                            
                            bo.Details.ForEach(d => dbContext.Database.ExecuteSqlCommand("exec UpdateOrderWhenDelivery {0}, {1},{2}", bo.Bill.ToOrganizationID, d.ProductID, d.Quantity));
                        }
                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }

        public OPResult SaveBillGoodReturn(BillBO<BillGoodReturn, BillGoodReturnDetails> bo)
        {
            using (var dbContext = new DistributionEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        BillHelper.SaveBill<BillGoodReturn, BillGoodReturnDetails>(bo, dbContext);
                        var storeout = BillHelper.GenerateStoreOut(bo);
                        BillHelper.SaveBillStoreOut(storeout, dbContext);
                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }

        public OPResult StoringReturnGoodReject(BillBO<BillGoodReturn, BillGoodReturnDetails> bo)
        {
            using (var dbContext = new DistributionEntities())
            {
                dbContext.Entry(bo.Bill).State = EntityState.Modified;
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        //BillHelper.SaveBill<BillGoodReturn, BillGoodReturnDetails>(bo, dbContext);
                        var storing = BillHelper.GenerateStoring(bo);
                        BillHelper.SaveBillStoring(storing, dbContext);
                        //if (bo.FundAccount != null && bo.FundAccount.AlreadyIn != 0)
                        //{
                        //    bo.FundAccount.RefrenceBillCode = bo.Bill.Code;
                        //    dbContext.OrganizationFundAccount.Add(bo.FundAccount);
                        //}
                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "入库失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult { IsSucceed = true, Message = "入库成功!" };
        }

        public OPResult<string> SaveBillCannibalize(BillBO<BillCannibalize, BillCannibalizeDetails> bo)
        {
            using (var dbContext = new DistributionEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        BillHelper.SaveBill<BillCannibalize, BillCannibalizeDetails>(bo, dbContext);
                        var storeout = BillHelper.GenerateStoreOut(bo);
                        BillHelper.SaveBillStoreOut(storeout, dbContext);

                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult<string> { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult<string> { IsSucceed = true, Message = "保存成功!", Result = bo.Bill.Code };
        }
    }
}