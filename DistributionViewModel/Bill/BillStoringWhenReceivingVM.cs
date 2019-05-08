using DistributionModel;
using DomainLogicEncap;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace DistributionViewModel
{
    public class BillStoringWhenReceivingVM : BillStoringWhenReceivingVMBase
    {
        public BillStoringWhenReceivingVM(BillWithBrand bill) : base(bill) { }

        /// <summary>
        /// 发货单查询(机构收货入库使用)
        /// </summary>
        public static List<DeliverySearchEntity> SearchBillDeliveryForStoring()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var deliveryContext = lp.GetDataContext<BillDelivery>();
            var deliveryDetailContext = lp.GetDataContext<BillDeliveryDetails>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            int status = (int)BillDeliveryStatusEnum.在途中;
            var dataQuery = from d in deliveryContext
                            where d.ToOrganizationID == VMGlobal.CurrentUser.OrganizationID && d.Status == status && brandIDs.Contains(d.BrandID)
                            select new DeliverySearchEntity
                            {
                                ID = d.ID,
                                Remark = d.Remark,
                                Code = d.Code,
                                CreateTime = d.CreateTime,
                                BrandID = d.BrandID
                            };
            var deliveries = dataQuery.ToList();
            var bIDs = deliveries.Select(o => (int)o.ID);
            var detailQuery = from detail in deliveryDetailContext
                              where bIDs.Contains(detail.BillID)
                              select new
                              {
                                  detail.BillID,
                                  detail.ProductID,
                                  detail.Quantity
                              };
            var details = detailQuery.ToList();
            var pids = details.Select(o => o.ProductID).ToArray();
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            //var sum = deliveryDetailContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            var sum = details.GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            deliveries.ForEach(d =>
            {
                d.BrandName = VMGlobal.PoweredBrands.Find(o => o.ID == d.BrandID).Name;
                d.Quantity = sum.Find(o => o.BillID == d.ID).Quantity;
                var tempDetails = details.FindAll(o => o.BillID == d.ID);
                foreach (var detail in tempDetails)
                {
                    var product = products.First(p => p.ProductID == detail.ProductID);
                    var price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, product.BYQID, product.Price);
                    d.TotalPrice += price * detail.Quantity;
                }
            });
            return deliveries;
        }

        public override OPResult Save()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            BillDelivery delivery = lp.Search<BillDelivery>(o => o.Code == Master.RefrenceBillCode).First();
            delivery.Status = (int)BillDeliveryStatusEnum.已入库;
            //var uniqueCodes = GetSnapshotDetails(delivery.ID);
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    base.SaveWithNoTran();
                    lp.Update<BillDelivery>(delivery);
                    Details.ForEach(d => BillLogic.AddStock(Master.StorageID, d.ProductID, d.Quantity));
                    //SaveUniqueCodes(uniqueCodes);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }

        protected override IEnumerable<ProductForStoringWhenReceiving> GetBillReceiveDetails(int billID)
        {
            return ReportDataContext.GetBillDeliveryDetails<ProductForStoringWhenReceiving>(billID);
        }
    }
}
