using DistributionModel;
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
using ViewModelBasic;

namespace DistributionViewModel
{
    public class StoringReturnGoodRejectVM : PagedReportVM<BillGoodReturnForSearch>
    {
        public StoringReturnGoodRejectVM()
        {
            this.Entities = this.SearchData();
        }

        protected override IEnumerable<BillGoodReturnForSearch> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            int status = (int)BillGoodReturnStatusEnum.被退回;
            var deliveryContext = lp.Search<BillGoodReturn>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Status == status && brandIDs.Contains(o.BrandID));
            var userContext = lp.GetDataContext<ViewUser>();
            var storageContext = lp.GetDataContext<Storage>();
            var billData = from d in deliveryContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           from s in storageContext 
                           where d.StorageID == s.ID 
                           select new BillGoodReturnForSearch
                           {
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime,
                               CreateDate = d.CreateTime.Date,
                               CreatorName = user.Name,
                               Status = d.Status,
                               StorageName = s.Name,
                               StorageID = d.StorageID,
                               TotalPrice = d.TotalPrice,
                               Quantity = d.Quantity
                           };

            TotalCount = billData.Count();
            var goodreturns = billData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = goodreturns.Select(o => (int)o.ID);
            var detailsContext = lp.GetDataContext<BillGoodReturnDetails>();
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            goodreturns.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(o => d.BrandID == o.ID).Name;
                //var details = sum.Find(o => o.BillID == d.ID);
                //d.Quantity = details.Quantity;
            });
            return new ObservableCollection<BillGoodReturnForSearch>(goodreturns);
        }

        public OPResult Storing(BillGoodReturnForSearch entity)
        {
            if (entity.StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "由于没有对应的出库仓库, 该单无法入库." };
            }
            var lp = VMGlobal.DistributionQuery.LinqOP;
            BillGoodReturn bill = lp.GetById<BillGoodReturn>(entity.ID);
            if (bill.Status == (int)BillGoodReturnStatusEnum.退回已入库)
                return new OPResult { IsSucceed = false, Message = "该退回单据已入库" };
            bill.Status = (int)BillGoodReturnStatusEnum.退回已入库;

            //decimal returnMoney = 0;
            //if (!OrganizationListVM.IsSelfRunShop(bill.OrganizationID))
            //{
            //    returnMoney = bill.TotalPrice;
            //}
            var bo = new BillBO<BillGoodReturn, BillGoodReturnDetails>
            {
                Bill = bill,
                Details = entity.Details.Select(o => new BillGoodReturnDetails
                {
                    ProductID = o.ProductID,
                    Quantity = o.Quantity
                }).ToList(),
            };
            //if (returnMoney != 0)
            //{
            //    bo.FundAccount =
            //        new OrganizationFundAccount
            //        {
            //            BrandID = bill.BrandID,
            //            OrganizationID = bill.OrganizationID,
            //            NeedIn = 0,
            //            AlreadyIn = returnMoney,
            //            CreatorID = VMGlobal.CurrentUser.ID,
            //            BillKind = (int)BillTypeEnum.BillGoodReturn,
            //            Remark = "退货入库生成,退货单号" + bill.Code,
            //            RefrenceBillCode = bill.Code
            //        };
            //}
            var result = BillWebApiInvoker.Instance.Invoke<OPResult, BillBO<BillGoodReturn, BillGoodReturnDetails>>(bo, "Bill/StoringReturnGoodReject");
            if (result.IsSucceed)
            {
                ObservableCollection<BillGoodReturnForSearch> entities = this.Entities as ObservableCollection<BillGoodReturnForSearch>;
                if (entities != null)
                    entities.Remove(entity);
            }
            return result;
        }
    }
}
