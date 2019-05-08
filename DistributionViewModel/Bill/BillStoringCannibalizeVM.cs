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
    public class BillStoringCannibalizeVM : BillStoringWhenReceivingVMBase
    {
        public BillStoringCannibalizeVM(BillWithBrand bill)
            : base(bill)
        { }

        /// <summary>
        /// 调拨单查询(机构收货入库使用)
        /// </summary>
        public static List<CannibalizeSearchEntity> SearchBillCannibalizeForStoring()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var cannibalizeContext = lp.GetDataContext<BillCannibalize>();
            var detailContext = lp.GetDataContext<BillCannibalizeDetails>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var dataQuery = from d in cannibalizeContext
                            where d.ToOrganizationID == VMGlobal.CurrentUser.OrganizationID && !d.Status && brandIDs.Contains(d.BrandID)
                            select new CannibalizeSearchEntity
                            {
                                ID = d.ID,
                                OrganizationID = d.OrganizationID,
                                Remark = d.Remark,
                                Code = d.Code,
                                CreateTime = d.CreateTime,
                                BrandID = d.BrandID
                            };
            var cannibalizes = dataQuery.ToList();
            var bIDs = cannibalizes.Select(o => (int)o.ID);
            var detailQuery = from detail in detailContext
                              where bIDs.Contains(detail.BillID)
                              select new
                              {
                                  detail.BillID,
                                  detail.ProductID,
                                  detail.Quantity
                              };
            var details = detailQuery.ToList();
            //var sum = cannibalizeDetailContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            var sum = details.GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            var siblings = OrganizationLogic.GetSiblingOrganizations(VMGlobal.CurrentUser.OrganizationID);
            cannibalizes.ForEach(d =>
            {
                d.OrganizationName = siblings.Find(o => o.ID == d.OrganizationID).Name;
                d.BrandName = VMGlobal.PoweredBrands.Find(o => o.ID == d.BrandID).Name;
                d.Quantity = sum.Find(o => o.BillID == d.ID).Quantity;
                var tempDetails = details.FindAll(o => o.BillID == d.ID);
                foreach (var detail in tempDetails)
                {
                    var price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, detail.ProductID);
                    d.TotalPrice += price * detail.Quantity;
                }
            });
            return cannibalizes;
        }

        /// <summary>
        /// 调拨单明细
        /// </summary>
        protected override IEnumerable<ProductForStoringWhenReceiving> GetBillReceiveDetails(int billID)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var detailsContext = lp.Search<BillCannibalizeDetails>(o => o.BillID == billID);
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new ProductForStoringWhenReceiving
                       {
                           ProductID = product.ProductID,
                           ProductCode = product.ProductCode,
                           StyleCode = product.StyleCode,
                           BYQID = product.BYQID,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Quantity = details.Quantity
                       };
            var result = data.ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.BrandID = VMGlobal.BYQs.Find(o => o.ID == r.BYQID).BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
            }
            return result;
        }

        public override OPResult Save()
        {
            BillCannibalize cannibalize = VMGlobal.DistributionQuery.LinqOP.Search<BillCannibalize>(o => o.Code == Master.RefrenceBillCode).First();
            cannibalize.Status = true;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    base.SaveWithNoTran();
                    VMGlobal.DistributionQuery.LinqOP.Update<BillCannibalize>(cannibalize);
                    Details.ForEach(d => BillLogic.AddStock(Master.StorageID, d.ProductID, d.Quantity));
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }
    }
}
