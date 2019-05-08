using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionViewModel;
using DistributionModel;
using System.Collections.ObjectModel;
using DomainLogicEncap;
using System.Collections;
using System.Transactions;
using Telerik.Windows.Data;
using SysProcessModel;
using Kernel;
using ERPViewModelBasic;
using SysProcessViewModel;
using IWCFServiceForIM;
using System.Data;
using ERPModelBO;

namespace DistributionViewModel
{
    public class ProductForCannibalize : DistributionProductShow
    {
        private int _outStorageStock;
        /// <summary>
        /// 移出仓库库存
        /// </summary>
        public int OutStorageStock
        {
            get { return _outStorageStock; }
            set
            {
                if (_outStorageStock != value)
                {
                    _outStorageStock = value;
                    OnPropertyChanged("OutStorageStock");
                }
            }
        }
    }

    /// <summary>
    /// 调拨单查询实体
    /// </summary>
    public class CannibalizeSearchEntity : BillCannibalize
    {
        /// <summary>
        /// 开单人姓名
        /// </summary>
        public string CreatorName { get; set; }

        public DateTime CreateDate { get; set; }

        public string BrandName { get; set; }

        public string OrganizationName { get; set; }

        /// <summary>
        /// 下级机构名称
        /// </summary>
        public string ToOrganizationName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string StatusName { get { return Status ? "已入库" : "在途中"; } }

        /// <summary>
        /// 出货仓库
        /// </summary>
        public string StorageName { get; set; }

        /// <summary>
        /// 调拨数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 调拨方向
        /// <remarks>true:调出 false:调入</remarks>
        /// </summary>
        public bool Direction { get; set; }

        public decimal TotalPrice { get; set; }
    }

    public class CannibalizeEntityForAggregation : StatusBillEntityForAggregation
    {
        public int ToOrganizationID { get; set; }

        /// <summary>
        /// 调拨方向
        /// <remarks>true:调出 false:调入</remarks>
        /// </summary>
        public bool Direction { get; set; }
    }

    public class CannibalizeDistributionEntity : DistributionProductShow
    {
        public string OutOrganizationName { get; set; }
        public string InOrganizationName { get; set; }
    }

    public class BillCannibalizeVM : DistributionBillVM<BillCannibalize, BillCannibalizeDetails, ProductForCannibalize>
    {
        private IEnumerable<SysOrganization> _organizationsToCannibalizeIn = null;

        public IEnumerable<SysOrganization> OrganizationsToCannibalizeIn
        {
            get
            {
                if (_organizationsToCannibalizeIn == null)
                    _organizationsToCannibalizeIn = OrganizationLogic.GetSiblingOrganizations(VMGlobal.CurrentUser.OrganizationID);
                return _organizationsToCannibalizeIn;
            }
        }

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

        public BillCannibalizeVM()
        {
            if (Brands != null && Brands.Count == 1)
                Master.BrandID = Brands[0].ID;
            if (Storages != null && Storages.Count == 1)
                Master.StorageID = Storages[0].ID;
            this.GridDataItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GridDataItems_CollectionChanged);
        }

        void GridDataItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                List<int> pids = new List<int>();
                TraverseDatas(e.NewItems, p => pids.Add(p.ProductID));
                if (Master.StorageID != default(int))
                {
                    var stocks = StockLogic.GetStockInStorage(Master.StorageID, productIDs: pids.ToArray());
                    TraverseDatas(e.NewItems, p =>
                    {
                        var stock = stocks.Find(s => s.ProductID == p.ProductID);
                        p.OutStorageStock = (stock == null ? 0 : stock.Quantity);
                    });
                }
            }
        }

        private void TraverseDatas(IEnumerable items, Action<ProductForCannibalize> action)
        {
            foreach (var item in items)
            {
                ProductForCannibalize p = (ProductForCannibalize)item;
                action(p);
            }
        }

        public OPResult ValidateWhenSave()
        {
            if (Master.StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择出货仓库" };
            }
            if (Master.ToOrganizationID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择调入机构" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择调拨品牌" };
            }
            return new OPResult { IsSucceed = true };
        }
        
        public override OPResult Save()
        {
            Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            var details = this.Details = new List<BillCannibalizeDetails>();
            TraverseGridDataItems(p =>
            {
                details.Add(new BillCannibalizeDetails { ProductID = p.ProductID, Quantity = p.Quantity });
            });
            if (details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }

            var opresult = BillWebApiInvoker.Instance.SaveBill<OPResult<string>,BillCannibalize, BillCannibalizeDetails>(new BillBO<BillCannibalize, BillCannibalizeDetails>()
            {
                Bill = this.Master,
                Details = this.Details
            });
            if (opresult.IsSucceed)
            {
                this.Master.Code = ((OPResult<string>)opresult).Result;
                var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == Master.ToOrganizationID || o.OrganizationID == OrganizationListVM.CurrentOrganization.ParentID).ToArray();
                var toName = VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(Master.ToOrganizationID).Name;
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("从{2}到{3}调拨{0}件,单号{1},到货后请及时入库.", Details.Sum(o => o.Quantity), Master.Code, OrganizationListVM.CurrentOrganization.Name, toName)
                }, IMReceiveAccessEnum.调拨单);
            }
            return opresult;
        }
    }

    public static partial class ReportDataContext
    {
        /// <summary>
        /// 调拨单明细
        /// </summary>
        public static List<DistributionProductShow> GetBillCannibalizeDetails(int billID)
        {
            var detailsContext = _query.LinqOP.Search<BillCannibalizeDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new DistributionProductShow
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
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == byq.BrandID).Code;
            }
            return result;
        }

        /// <summary>
        /// 下级调拨单汇总
        /// </summary>
        public static DataTable AggregateSubordinateCannibalize(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var oids = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Select(o => o.ID).ToArray();
            var cannibalizeContext = lp.Search<BillCannibalize>(o => oids.Contains(o.ToOrganizationID) && oids.Contains(o.OrganizationID));
            var detailsContext = lp.GetDataContext<BillCannibalizeDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            FilterBillWithBrand(cannibalizeContext, filters, brandIDs);
            var data = from cannibalize in cannibalizeContext
                       from details in detailsContext
                       where cannibalize.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new CannibalizeEntityForAggregation
                       {
                           OrganizationID = cannibalize.OrganizationID,
                           ToOrganizationID = cannibalize.ToOrganizationID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = cannibalize.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           Status = cannibalize.Status
                       };

            data = (IQueryable<CannibalizeEntityForAggregation>)data.Where(filters);
            return new BillReportHelper().TransferSizeToHorizontal<DistributionProductShow>(AggregateBill(data, false));
        }

        /// <summary>
        /// 下级调拨单分布
        /// </summary>
        public static List<CannibalizeDistributionEntity> GetSubordinateCannibalizeDistribution(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = childOrganizations.Select(o => o.ID).ToArray();
            var cannibalizeContext = lp.Search<BillCannibalize>(o => oids.Contains(o.ToOrganizationID) && oids.Contains(o.OrganizationID));
            var detailsContext = lp.GetDataContext<BillCannibalizeDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            FilterBillWithBrand(cannibalizeContext, filters, brandIDs);
            var data = from cannibalize in cannibalizeContext
                       from details in detailsContext
                       where cannibalize.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new CannibalizeEntityForAggregation
                       {
                           OrganizationID = cannibalize.OrganizationID,
                           ToOrganizationID = cannibalize.ToOrganizationID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = cannibalize.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           Status = cannibalize.Status
                       };
            var filtedData = (IQueryable<CannibalizeEntityForAggregation>)data.Where(filters);
            var sum = filtedData.GroupBy(o => new { o.ProductID, o.OrganizationID, o.ToOrganizationID }).Select(g => new
            {
                Key = g.Key,
                Quantity = g.Sum(o => o.Quantity)
            }).ToList();
            var pids = sum.Select(o => o.Key.ProductID).ToArray();
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = sum.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key.ProductID);
                return new CannibalizeDistributionEntity
                {
                    BYQID = product.BYQID,
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    OutOrganizationName = childOrganizations.Find(s => s.ID == o.Key.OrganizationID).Name,
                    InOrganizationName = childOrganizations.Find(s => s.ID == o.Key.ToOrganizationID).Name
                };
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                r.BrandID = byq.BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
            }
            return result;
        }
    }
}
