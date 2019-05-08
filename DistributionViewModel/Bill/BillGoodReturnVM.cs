using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.Transactions;
using Telerik.Windows.Data;
using DistributionViewModel;
using DomainLogicEncap;
using SysProcessModel;
using Kernel;
using ERPViewModelBasic;
using SysProcessViewModel;
using ViewModelBasic;
using ManufacturingModel;
using IWCFServiceForIM;
using System.Data;
using System.Collections;
using ERPModelBO;

namespace DistributionViewModel
{
    //public class GoodReturnSearchEntity : BillGoodReturn
    //{
    //    public string OrganizationName { get; set; }
    //    public string BrandName { get; set; }
    //    public DateTime CreateDate { get; set; }
    //    public string CreatorName { get; set; }
    //    public string StorageName { get; set; }
    //    public int Quantity { get; set; }
    //    public string StatusName { get { return Status ? "已入库" : "在途中"; } }
    //}

    public class BillGoodReturnVM : GoodReturnForSubordinateVM
    {
        public List<ProBrand> Brands
        {
            get
            {
                return VMGlobal.PoweredBrands;
            }
        }

        public List<Storage> Storages
        {
            get { return StorageInfoVM.Storages; }
        }

        public BillGoodReturnVM()
        {
            Master = new BillGoodReturnBO();
            Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            if (Brands != null && Brands.Count == 1)
                Master.BrandID = Brands[0].ID;
            if (Storages != null && Storages.Count == 1)
                Master.StorageID = Storages[0].ID;
        }

        protected override decimal GetFloatPrice(GoodReturnProductShow product)
        {
            return FPHelper.GetFloatPrice(OrganizationListVM.CurrentOrganization.ParentID, product.BYQID, product.Price);
        }

        public override OPResult ValidateWhenSave()
        {
            if (Master.StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择出货仓库" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择退货品牌" };
            }
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            if (!OrganizationListVM.IsSelfRunShop(VMGlobal.CurrentUser.OrganizationID))
            {
#if UniqueCode
                List<string> uniqueCodes = new List<string>();
                TraverseGridDataItems(
                    item =>
                    {
                        if (item.UniqueCodes.Count > 0)
                        {
                            uniqueCodes.AddRange(item.UniqueCodes);
                        }
                    }
                );
                var ucarray = uniqueCodes.ToArray();
                var result = this.CheckDataAvaliable(ucarray);
                if (!result.IsSucceed)
                    return result;
                result = 
#else
                var result =
#endif
 this.CheckGoodReturnRate(VMGlobal.CurrentUser.OrganizationID);
                if (!result.IsSucceed)
                    return result;
            }
            this.Master.TotalPrice = this.GridDataItems.Sum(o => o.Quantity * o.Price * o.Discount) / 100;//本单退货金额
            this.Master.Quantity = this.GridDataItems.Sum(o => o.Quantity);            
            var details = this.Details = new List<BillGoodReturnDetails>();
            TraverseGridDataItems(p =>
            {
                details.Add(new BillGoodReturnDetails { ProductID = p.ProductID, Quantity = p.Quantity, Discount = p.Discount, Price = p.Price });
            });
            if (details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            var opresult = BillWebApiInvoker.Instance.SaveBill<BillGoodReturn, BillGoodReturnDetails>(new BillBO<BillGoodReturn, BillGoodReturnDetails>() {
                Bill = this.Master,
                Details = this.Details
            });
            if (opresult.IsSucceed)
            {
                var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == OrganizationListVM.CurrentOrganization.ParentID).ToArray();
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("{2}退货{0}件,单号{1},请注意查收.", Details.Sum(o => o.Quantity), Master.Code, OrganizationListVM.CurrentOrganization.Name)
                }, IMReceiveAccessEnum.退货单);
                this.Init();
            }
            return opresult;
        }

        public override void Init()
        {
            base.Init();
            Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }
    }

    public static partial class ReportDataContext
    {
        /// <summary>
        /// 本级退货单汇总
        /// </summary>
        public static DataTable AggregateSelfGoodReturn(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var goodReturnContext = lp.Search<BillGoodReturn>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            var detailsContext = lp.GetDataContext<BillGoodReturnDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);

            FilterBillWithBrand(goodReturnContext, filters, brandIDs);
            var data = from goodreturn in goodReturnContext
                       from details in detailsContext
                       where goodreturn.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new MultiStatusBillEntityForAggregation
                       {
                           StorageID = goodreturn.StorageID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = goodreturn.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           Status = goodreturn.Status
                       };

            data = (IQueryable<MultiStatusBillEntityForAggregation>)data.Where(filters);
            return new BillReportHelper().TransferSizeToHorizontal<DistributionProductShow>(AggregateBill(data));
        }
    }
}
