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
using System.Transactions;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class AuditingGoodReturnForSubordinateVM : PagedReportVM<BillGoodReturnForSearch>
    {
        public AuditingGoodReturnForSubordinateVM()
        {
            this.Entities = this.SearchData();
        }

        protected override IEnumerable<BillGoodReturnForSearch> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Select(o => o.ID).ToArray();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            int status = (int)BillGoodReturnStatusEnum.未审核;
            var goodreturnContext = lp.Search<BillGoodReturn>(o => oids.Contains(o.OrganizationID) && o.Status == status && brandIDs.Contains(o.BrandID));
            var userContext = lp.GetDataContext<ViewUser>();

            var billData = from d in goodreturnContext
                           from u in userContext
                           where d.CreatorID == u.ID
                           select new BillGoodReturnForSearch
                           {
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime,
                               CreateDate = d.CreateTime.Date,
                               Status = d.Status,
                               OrganizationID = d.OrganizationID,
                               TotalPrice = d.TotalPrice,
                               Quantity = d.Quantity,
                               StorageID = d.StorageID,
                               CreatorName = u.Name
                           };
            var detailsContext = lp.GetDataContext<BillGoodReturnDetails>();
            TotalCount = billData.Count();
            var goodreturns = billData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = goodreturns.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            goodreturns.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(o => d.BrandID == o.ID).Name;
                d.OrganizationName = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.First(o => o.ID == d.OrganizationID).Name;
                d.StorageName = StorageInfoVM.Storages.FirstOrEmpty(o => o.ID == (-1) * d.StorageID).Name;
            });
            return new ObservableCollection<BillGoodReturnForSearch>(goodreturns);
        }

        public OPResult Auditing(BillGoodReturnForSearch bill)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var gr = lp.GetById<BillGoodReturn>(bill.ID);
            if (gr.Status != (int)BillGoodReturnStatusEnum.未审核)
                return new OPResult { IsSucceed = false, Message = "该单据已审核" };
            gr.Status = (int)BillGoodReturnStatusEnum.在途中;

            OPResult result = null;
            try
            {
                lp.Update<BillGoodReturn>(gr);
                result = new OPResult { IsSucceed = true, Message = "审核成功!" };
            }
            catch (Exception ex)
            {
                result = new OPResult { IsSucceed = false, Message = ex.Message };
            }
            if (result.IsSucceed)
            {
                ObservableCollection<BillGoodReturnForSearch> entities = this.Entities as ObservableCollection<BillGoodReturnForSearch>;
                if (entities != null)
                    entities.Remove(bill);
            }
            return result;
        }

        public OPResult Reject(BillGoodReturnForSearch bill)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var gr = lp.GetById<BillGoodReturn>(bill.ID);
            if (gr.Status == (int)BillGoodReturnStatusEnum.被退回)
                return new OPResult { IsSucceed = false, Message = "该单据已驳回" };
            gr.Status = (int)BillGoodReturnStatusEnum.被退回;
            try
            {
                lp.Update<BillGoodReturn>(gr);
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "驳回失败:" + e.Message };
            }

            ObservableCollection<BillGoodReturnForSearch> entities = this.Entities as ObservableCollection<BillGoodReturnForSearch>;
            if (entities != null)
                entities.Remove(bill);
            return new OPResult { IsSucceed = true, Message = "驳回成功" };
        }
    }
}
