using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using SysProcessModel;
using ManufacturingModel;
using Manufacturing.ViewModel;
using Kernel;
using System.Collections.ObjectModel;
using System.Transactions;
using DistributionModel;
using ViewModelBasic;
using SysProcessViewModel;
using ERPModelBO;

namespace DistributionViewModel
{
    public class StoringProductExchangeVM : CommonViewModel<BillStoringProductExchangeEntity>
    {
        public StoringProductExchangeVM()
        {
            this.Entities = this.SearchData();
        }

        protected override IEnumerable<BillStoringProductExchangeEntity> SearchData()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var productExchangeContext = lp.GetDataContext<BillProductExchange>();
            var productExchangeDetailsContext = lp.GetDataContext<BillProductExchangeDetails>();
            //var userContext = lp.GetDataContext<ViewUser>();

            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            int status = (int)BillProductExchangeStatusEnum.在途中;
            var billData = from pe in productExchangeContext
                           where pe.Status == status && brandIDs.Contains(pe.BrandID) && pe.OrganizationID == VMGlobal.CurrentUser.OrganizationID && !pe.IsDeleted
                           select new BillStoringProductExchangeEntity
                           {
                               CreateTime = pe.CreateTime,
                               Status = pe.Status,
                               ID = pe.ID,
                               BrandID = pe.BrandID,
                               Code = pe.Code,
                               Remark = pe.Remark
                           };
            var datas = billData.ToList();
            var bIDs = datas.Select(o => (int)o.ID);
            var sum = productExchangeDetailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, TotalQuantity = g.Sum(o => o.Quantity) }).ToList();
            datas.ForEach(o =>
            {
                o.BrandName = brands.Find(b => b.ID == o.BrandID).Name;
                o.Quantity = sum.Find(s => s.BillID == o.ID).TotalQuantity;
            });
            return new ObservableCollection<BillStoringProductExchangeEntity>(datas);
        }

        public OPResult SendBack(BillStoringProductExchangeEntity entity)
        {
            if (entity.Status == (int)BillProductExchangeStatusEnum.已入库)
            {
                return new OPResult { IsSucceed = false, Message = "已入库单据不能退回." };
            }
            if (entity.Status == (int)BillProductExchangeStatusEnum.被退回)
            {
                return new OPResult { IsSucceed = false, Message = "该单已退回." };
            }
            BillProductExchange pe = VMGlobal.ManufacturingQuery.LinqOP.GetById<BillProductExchange>(entity.ID);
            pe.Status = (int)BillProductExchangeStatusEnum.被退回;
            pe.Remark = entity.Remark;
            try
            {
                VMGlobal.ManufacturingQuery.LinqOP.Update<BillProductExchange>(pe);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "退回失败\n失败原因:" + e.Message };
            }
            entity.Status = (int)BillProductExchangeStatusEnum.被退回;
            ((ObservableCollection<BillStoringProductExchangeEntity>)this.Entities).Remove(entity);
            return new OPResult { IsSucceed = true, Message = "退回成功." };
        }

        public OPResult Storing(BillStoringProductExchangeEntity entity)
        {
            if (entity.Status == (int)BillProductExchangeStatusEnum.已入库)
            {
                return new OPResult { IsSucceed = false, Message = "该单已经入库." };
            }
            if (entity.Status == (int)BillProductExchangeStatusEnum.被退回)
            {
                return new OPResult { IsSucceed = false, Message = "被退回单据不能入库." };
            }
            if (entity.StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择入库仓库." };
            }
            BillStoringVM storingvm = this.GenerateStoring(entity);
            BillProductExchange pe = VMGlobal.ManufacturingQuery.LinqOP.GetById<BillProductExchange>(entity.ID);
            pe.Status = (int)BillProductExchangeStatusEnum.已入库;
#if UniqueCode
            var uniqueCodes = BillStoringVM.GetSnapshotDetails(pe.Code);
#endif
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    storingvm.SaveWithNoTran();
                    VMGlobal.ManufacturingQuery.LinqOP.Update<BillProductExchange>(pe);
#if UniqueCode
                    storingvm.SaveUniqueCodes(uniqueCodes);
#endif
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "入库失败\n失败原因:" + e.Message };
                }
            }
            entity.Status = (int)BillProductExchangeStatusEnum.已入库;
            ((ObservableCollection<BillStoringProductExchangeEntity>)this.Entities).Remove(entity);
            return new OPResult { IsSucceed = true, Message = "入库成功." };
        }

        /// <summary>
        /// 生成入库单
        /// </summary>
        private BillStoringVM GenerateStoring(BillStoringProductExchangeEntity entity)
        {
            BillStoringVM storing = new BillStoringVM();
            BillStoring bill = storing.Master;
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            bill.StorageID = entity.StorageID;
            bill.RefrenceBillCode = entity.Code;
            bill.BillType = (int)BillTypeEnum.BillProductExchange;
            bill.Remark = "交接入库";
            bill.BrandID = entity.BrandID;

            List<BillStoringDetails> siDetails = new List<BillStoringDetails>();
            foreach (var p in entity.Details)
            {
                siDetails.Add(new BillStoringDetails
                {
                    ProductID = p.ProductID,
                    Quantity = p.Quantity
                });
            };
            storing.Details = siDetails;
            return storing;
        }
    }
}
