using DistributionModel;
using DomainLogicEncap;
using ERPModelBO;
using ERPViewModelBasic;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Transactions;

namespace DistributionViewModel
{
    public class BillStoringReturnGoodVM : BillStoringWhenReceivingVMBase
    {
        /// <summary>
        /// 退货金额
        /// </summary>
        private decimal ReturnMoney { get; set; }

        public BillStoringReturnGoodVM(BillWithBrand bill)
            : base(bill)
        { }

        public static ObservableCollection<BillGoodReturnForSearch> SearchBillSubordinateGoodReturnForStoring()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = childOrganizations.Select(o => o.ID);
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            int status = (int)BillGoodReturnStatusEnum.在途中;
            var goodreturnContext = lp.Search<BillGoodReturn>(o => o.Status == status && oids.Contains(o.OrganizationID) && brandIDs.Contains(o.BrandID));
            var billData = from d in goodreturnContext
                           select new BillGoodReturnForSearch
                           {
                               ID = d.ID,
                               OrganizationID = d.OrganizationID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime
                           };
            var goodreturns = billData.ToList();
            var detailsContext = lp.GetDataContext<BillGoodReturnDetails>();
            var bIDs = goodreturns.Select(o => (int)o.ID);
            var detailQuery = from detail in detailsContext
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
            //var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            var sum = details.GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            goodreturns.ForEach(d =>
            {
                d.OrganizationName = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Find(o => o.ID == d.OrganizationID).Name;
                d.BrandName = brands.Find(o => d.BrandID == o.ID).Name;
                d.Quantity = sum.Find(o => o.BillID == d.ID).Quantity;
                var tempDetails = details.FindAll(o => o.BillID == d.ID);
                foreach (var detail in tempDetails)
                {
                    var product = products.First(p => p.ProductID == detail.ProductID);
                    var price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, product.BYQID, product.Price);
                    d.TotalPrice += price * detail.Quantity;
                }
            });
            return new ObservableCollection<BillGoodReturnForSearch>(goodreturns);
        }

        public override OPResult Save()
        {
            BillGoodReturn goodreturn = VMGlobal.DistributionQuery.LinqOP.Search<BillGoodReturn>(o => o.Code == Master.RefrenceBillCode).First();
            if (goodreturn.Status == (int)BillGoodReturnStatusEnum.已入库)
                return new OPResult { IsSucceed = false, Message = "该单已入库" };
            //var uniqueCodes = GetSnapshotDetails(goodreturn.ID);
            if (!OrganizationListVM.IsSelfRunShop(goodreturn.OrganizationID))//检查当初发这些货品的时候的发货价.另:非自营店不能退不是发给自己的货物
            {
                //var index = goodreturn.Remark.LastIndexOf(':');
                //if (index != -1)
                //{
                ReturnMoney = goodreturn.TotalPrice;//Convert.ToDecimal(goodreturn.Remark.Substring(index + 1));
                //}
            }
            goodreturn.Status = (int)BillGoodReturnStatusEnum.已入库;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    base.SaveWithNoTran();
                    VMGlobal.DistributionQuery.LinqOP.Update<BillGoodReturn>(goodreturn);
                    if (ReturnMoney != 0)
                    {
                        VMGlobal.DistributionQuery.LinqOP.Add<OrganizationFundAccount>(
                            new OrganizationFundAccount
                            {
                                BrandID = Master.BrandID,
                                OrganizationID = goodreturn.OrganizationID,//this.Master.OrganizationID,
                                NeedIn = 0,
                                AlreadyIn = ReturnMoney,
                                CreatorID = VMGlobal.CurrentUser.ID,
                                BillKind = (int)BillTypeEnum.BillGoodReturn,
                                Remark = "退货入库生成,退货单号" + goodreturn.Code,
                                RefrenceBillCode = this.Master.Code
                            });
                    }
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
            var detailData = ReportDataContext.SearchBillDetails<BillGoodReturnDetails>(billID);
            return detailData.Select(o => new ProductForStoringWhenReceiving
            {
                ProductID = o.ProductID,
                ProductCode = o.ProductCode,
                BrandCode = o.BrandCode,
                StyleCode = o.StyleCode,
                ColorCode = o.ColorCode,
                SizeName = o.SizeName,
                Quantity = o.Quantity,
                ReceiveQuantity = 0
            });
        }

        public static OPResult SendBack(BillGoodReturnForSearch entity)
        {
            if (entity.Status == (int)BillGoodReturnStatusEnum.已入库)
            {
                return new OPResult { IsSucceed = false, Message = "已入库单据不能退回." };
            }
            if (entity.Status == (int)BillGoodReturnStatusEnum.被退回)
            {
                return new OPResult { IsSucceed = false, Message = "该单已退回." };
            }
            var lp = VMGlobal.DistributionQuery.LinqOP;
            BillGoodReturn bill = lp.GetById<BillGoodReturn>(entity.ID);
            bill.Status = (int)BillGoodReturnStatusEnum.被退回;
            bill.Remark = entity.Remark;
            try
            {
                lp.Update<BillGoodReturn>(bill);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "退回失败\n失败原因:" + e.Message };
            }
            entity.Status = (int)BillGoodReturnStatusEnum.被退回;
            return new OPResult { IsSucceed = true, Message = "退回成功." };
        }
    }
}
