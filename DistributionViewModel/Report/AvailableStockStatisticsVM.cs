using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using DistributionModel;
using DomainLogicEncap;
using Telerik.Windows.Data;
using SysProcessModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class AvailableStockStatisticsVM : StockStatisticsVM
    {
        protected override IEnumerable<StockStatisticsEntity> SearchData()
        {
            var query = VMGlobal.DistributionQuery;
            var provider = query.QueryProvider;
            var lp = query.LinqOP;
            var stockContext = lp.GetDataContext<Stock>();
            var storageContext = lp.Search<Storage>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Flag);
            var productContext = provider.GetTable<Product>("SysProcess.dbo.Product");
            var styleContext = provider.GetTable<ProStyle>("SysProcess.dbo.ProStyle");
            var pnameContext = provider.GetTable<ProName>("SysProcess.dbo.ProName");
            var brandContext = provider.GetTable<ProBrand>("SysProcess.dbo.ProBrand");
            var colorContext = provider.GetTable<ProColor>("SysProcess.dbo.ProColor");
            var sizeContext = provider.GetTable<ProSize>("SysProcess.dbo.ProSize");
            var byqContext = provider.GetTable<ProBYQ>("SysProcess.dbo.ProBYQ");

            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);

            var data = from stock in stockContext
                       from storage in storageContext
                       where stock.StorageID == storage.ID
                       from product in productContext
                       where stock.ProductID == product.ID
                       from style in styleContext
                       where product.StyleID == style.ID
                       from byq in byqContext
                       where style.BYQID == byq.ID
                       from brand in brandContext
                       where byq.BrandID == brand.ID && brandIDs.Contains(brand.ID)
                       from color in colorContext
                       where product.ColorID == color.ID
                       from size in sizeContext
                       where product.SizeID == size.ID
                       from pname in pnameContext
                       where style.NameID == pname.ID
                       select new AvailableStockStatisticsEntity
                       {
                           StorageID = storage.ID,
                           NameID = pname.ID,
                           StyleCode = style.Code,
                           ColorCode = color.Code,
                           SizeID = size.ID,
                           BrandID = brand.ID,
                           Year = byq.Year,
                           Quarter = byq.Quarter,
                           ProductCode = product.Code,
                           StorageName = storage.Name,
                           BrandCode = brand.Code,
                           SizeName = size.Name,
                           ProductName = pname.Name,
                           Quantity = stock.Quantity,
                           ProductID = product.ID
                       };
            var result = ((IQueryable<AvailableStockStatisticsEntity>)data.Where(FilterDescriptors)).ToList();//即使filters中有data没有的过滤属性，也不会出错，但是会产生0<>0的恒为假条件
            var pids = result.Select(o => o.ProductID).Distinct();
            var sids = result.Select(o => o.StorageID).Distinct();

            var allocates = lp.Search<BillAllocate>(o => !o.Status && sids.Contains(o.StorageID));
            var allocateDetails = lp.GetDataContext<BillAllocateDetails>();
            var users = lp.GetDataContext<ViewUser>();
            var allocateData = from allocate in allocates
                               from details in allocateDetails
                               where allocate.ID == details.BillID && pids.Contains(details.ProductID)
                               from u in users
                               where allocate.CreatorID == u.ID
                               select new StockOccupationInfo
                               {
                                   StorageID = allocate.StorageID,
                                   BillCode = allocate.Code,
                                   BillID = allocate.ID,
                                   CreateTime = allocate.CreateTime,
                                   OrganizationID = allocate.OrganizationID,
                                   QuaOccupation = details.Quantity,
                                   CreatorName = u.Name,
                                   ProductID = details.ProductID
                               };
            var allocateResult = allocateData.ToList();
            var childrenOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            allocateResult.ForEach(o =>
            {
                var organization = childrenOrganizations.Find(c => c.ID == o.OrganizationID);
                o.OrganizationName = (organization == null ? "" : organization.Name);
                o.OrganizationCode = (organization == null ? "" : organization.Code);
                o.BillKind = 1;
            });

            int status = (int)BillDeliveryStatusEnum.已装箱未配送;
            var deliveries = lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Status == status && sids.Contains(o.StorageID));
            var deliveryDetails = lp.GetDataContext<BillDeliveryDetails>();
            var deliveryData = from delivery in deliveries
                               from details in deliveryDetails
                               where delivery.ID == details.BillID && pids.Contains(details.ProductID)
                               from u in users
                               where delivery.CreatorID == u.ID
                               select new StockOccupationInfo
                               {
                                   StorageID = delivery.StorageID,
                                   BillCode = delivery.Code,
                                   BillID = delivery.ID,
                                   CreateTime = delivery.CreateTime,
                                   OrganizationID = delivery.ToOrganizationID,
                                   QuaOccupation = details.Quantity,
                                   CreatorName = u.Name,
                                   ProductID = details.ProductID
                               };
            var deliveryResult = deliveryData.ToList();
            deliveryResult.ForEach(o =>
            {
                var organization = childrenOrganizations.Find(c => c.ID == o.OrganizationID);
                o.OrganizationName = (organization == null ? "" : organization.Name);
                o.OrganizationCode = (organization == null ? "" : organization.Code);
                o.BillKind = 2;
            });

            allocateResult.AddRange(deliveryResult);
            result.ForEach(o =>
            {                
                o.Details = allocateResult.Where(d => d.ProductID == o.ProductID && d.StorageID == o.StorageID);
                o.QuaOccupation = o.Details.Sum(d => d.QuaOccupation);
                o.QuaAvailable = o.Quantity - o.QuaOccupation;
                o.QuarterName = VMGlobal.Quarters.Find(q => q.ID == o.Quarter).Name;
            });
            return result;
        }
    }

    public class AvailableStockStatisticsEntity : StockStatisticsEntity
    {
        /// <summary>
        /// 被占用库存数量
        /// </summary>
        public int QuaOccupation { get; set; }
        public int QuaAvailable { get; set; }
        public bool IsShowDetails
        {
            get
            {
                return Details != null && Details.Count() > 0;
            }
        }
        public IEnumerable<StockOccupationInfo> Details
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 库存占用详情
    /// </summary>
    public class StockOccupationInfo
    {
        public int StorageID { get; set; }
        public int OrganizationID { get; set; }
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; }
        public int BillID { get; set; }
        public string BillCode { get; set; }
        public int QuaOccupation { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreateTime { get; set; }
        public int ProductID { get; set; }
        public int BillKind { get; set; }
        public string BillKindName {
            get { return BillKind == 1 ? "配货单" : "发货装箱"; }
        }
    }
}
