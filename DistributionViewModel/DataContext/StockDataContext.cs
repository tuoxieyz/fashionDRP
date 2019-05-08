using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using DomainLogicEncap;
using DistributionModel;
using DistributionViewModel;
using System.Collections;
using System.ComponentModel;
using ERPViewModelBasic;
using SysProcessModel;
using SysProcessViewModel;
using System.Data;


namespace DistributionViewModel
{
    public class StoringSaleStockEntity : DistributionProductShow
    {
        /// <summary>
        /// 期初库存
        /// </summary>
        public int OrginalQuantity { get; set; }

        public int 成品入库 { get; set; }
        public int 交接入库 { get; set; }
        public int 发货入库 { get; set; }
        public int 移库入库 { get; set; }
        public int 零售入库 { get; set; }
        public int 调拨入库 { get; set; }
        public int 退货入库 { get; set; }
        public int 发货出库 { get; set; }
        public int 移库出库 { get; set; }
        public int 零售出库 { get; set; }
        public int 调拨出库 { get; set; }
        public int 退货出库 { get; set; }
        public int 盈亏数量 { get; set; }
    }

    /// <summary>
    /// 出入库汇总实体
    /// </summary>
    internal class StoringOutEntityForAggregation : BillEntityForAggregation
    {
        /// <summary>
        /// 出入库涉及到的单据类型
        /// </summary>
        public int BillType { get; set; }
        /// <summary>
        /// 开单日期
        /// </summary>
        public DateTime CreateDate { get; set; }
    }

    public partial class ReportDataContext
    {
        #region 库存相关

        /// <summary>
        /// 入库单明细
        /// </summary>
        //public static IList GetBillStoringDetails(int billID)
        //{
        //    var detailsContext = _query.LinqOP.Search<BillStoringDetails>(o => o.BillID == billID);
        //    var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
        //    var data = from details in detailsContext
        //               from product in productContext
        //               where details.ProductID == product.ProductID
        //               select new ProductForBill
        //               {
        //                   ProductCode = product.ProductCode,
        //                   BrandCode = product.BrandCode,
        //                   StyleCode = product.StyleCode,
        //                   ColorCode = product.ColorCode,
        //                   SizeName = product.SizeName,
        //                   Price = product.Price,
        //                   Quantity = details.Quantity
        //               };
        //    return data.ToList();
        //}

        /// <summary>
        /// 入库汇总
        /// </summary>
        //public static DataTable AggregateStoring(CompositeFilterDescriptorCollection filters)
        //{
        //    var lp = _query.LinqOP;
        //    var storingContext = lp.GetDataContext<BillStoring>().Where(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
        //    var storingDetailsContext = lp.GetDataContext<BillStoringDetails>();
        //    var productContext = lp.GetDataContext<ViewProduct>();
        //    //var storageContext = lp.GetDataContext<Storage>();

        //    var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
        //    FilterBillWithBrand(storingContext, filters, brandIDs);
        //    var data = from storing in storingContext
        //               from storingDetails in storingDetailsContext
        //               where storing.ID == storingDetails.BillID
        //               from product in productContext
        //               where product.ProductID == storingDetails.ProductID //&& brandIDs.Contains(product.BrandID)
        //               //from storage in storageContext
        //               //where storing.StorageID == storage.ID
        //               select new StoreOIEntityForAggregation
        //               {
        //                   ProductID = product.ProductID,
        //                   BrandID = product.BrandID,
        //                   StorageID = storing.StorageID,
        //                   CreateTime = storing.CreateTime.Date,
        //                   BillType = storing.BillType,
        //                   //ProductCode = product.ProductCode,
        //                   //BrandCode = product.BrandCode,
        //                   StyleCode = product.StyleCode,
        //                   //ColorCode = product.ColorCode,
        //                   //SizeName = product.SizeName,
        //                   //Price = product.Price,
        //                   NameID = product.NameID,
        //                   Quantity = storingDetails.Quantity
        //               };
        //    data = (IQueryable<StoreOIEntityForAggregation>)data.Where(filters);
        //    return new BillReportHelper().TransferSizeToHorizontal<DistributionProductForBrush>(AggregateBill(data));
        //}

        #endregion

        public static IEnumerable<DistributionEntity> GetOtherShopStock(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var shops = OrganizationLogic.GetSiblingShops(VMGlobal.CurrentUser.OrganizationID);
            var oids = shops.Where(o => o.ID != VMGlobal.CurrentUser.OrganizationID).Select(o => o.ID);
            var stockContext = lp.GetDataContext<Stock>();
            var storageContext = lp.Search<Storage>(o => oids.Contains(o.OrganizationID) && o.Flag);
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var productContext = lp.Search<ViewProduct>(o => brandIDs.Contains(o.BrandID));

            var data = from stock in stockContext
                       from storage in storageContext
                       where stock.StorageID == storage.ID && stock.Quantity != 0
                       from product in productContext
                       where stock.ProductID == product.ProductID
                       select new BillEntityForAggregation
                       {
                           OrganizationID = storage.OrganizationID,
                           StyleCode = product.StyleCode,
                           BrandID = product.BrandID,
                           ProductID = product.ProductID,
                           Quantity = stock.Quantity
                       };
            var filtedData = (IQueryable<BillEntityForAggregation>)data.Where(filters);//即使filters中有data没有的过滤属性，也不会出错，但是会产生0<>0的恒为假条件
            var sum = filtedData.GroupBy(o => new { o.ProductID, o.OrganizationID }).Select(g => new
            {
                Key = g.Key,
                Quantity = g.Sum(o => o.Quantity)
            }).ToList();
            var pids = sum.Select(o => o.Key.ProductID).ToArray();
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = sum.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key.ProductID);
                return new DistributionEntity
                {
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    OrganizationID = o.Key.OrganizationID,
                    OrganizationName = shops.Find(c => c.ID == o.Key.OrganizationID).Name
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
            //return new BillReportHelper().TransferSizeToHorizontal<DistributionEntity>(result);
        }

        /// <summary>
        /// 获取可用库存
        /// </summary>
        internal static IEnumerable<ProductQuantity> GetAvailableStock(int storageID, IEnumerable<ProStyle> styles)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var provider = VMGlobal.DistributionQuery.QueryProvider;
            var stockContext = lp.Search<Stock>(o => o.StorageID == storageID && o.Quantity != 0);
            var productContext = provider.GetTable<Product>("SysProcess.dbo.Product");
            var styleContext = provider.GetTable<ProStyle>("SysProcess.dbo.ProStyle");

            var styleIDs = styles.Select(o => o.ID);

            var data = from stock in stockContext
                       from product in productContext
                       where stock.ProductID == product.ID
                       from style in styleContext
                       where product.StyleID == style.ID && styleIDs.Contains(style.ID)
                       select new ProductQuantity
                       {
                           Quantity = stock.Quantity,
                           ProductID = product.ID
                       };
            var stocks = data.ToList();//即使filters中有data没有的过滤属性，也不会出错，但是会产生0<>0的恒为假条件
            var pids = stocks.Select(o => o.ProductID);
            //int status = (int)BillDeliveryStatusEnum.已装箱未配送;
            //var deliveries = lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Status == status);
            //var deliveryDetails = lp.GetDataContext<BillDeliveryDetails>();
            //var deliveryData = from delivery in deliveries
            //                   from details in deliveryDetails
            //                   where delivery.ID == details.BillID && pids.Contains(details.ProductID)
            //                   select new ProductQuantity
            //                   {
            //                       Quantity = details.Quantity,
            //                       ProductID = details.ProductID
            //                   };
            ////占用库存
            //var deliveryResult = deliveryData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //stocks.ForEach(o =>
            //{
            //    var dtemp = deliveryResult.Find(d => d.ProductID == o.ProductID);
            //    o.Quantity = o.Quantity - (dtemp == null ? 0 : dtemp.Quantity);
            //});

            var allocateResult = ReportDataContext.GetOccupationStock(storageID, pids);
            stocks.ForEach(o =>
            {
                var dtemp = allocateResult.Find(d => d.ProductID == o.ProductID);
                o.Quantity = o.Quantity - (dtemp == null ? 0 : dtemp.Quantity);
            });
            return stocks;
        }

        internal static List<ProductQuantity> GetOccupationStock(int storageID, IEnumerable<int> productIDs)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var allocates = lp.Search<BillAllocate>(o => !o.Status && o.StorageID == storageID);
            var allocateDetails = lp.GetDataContext<BillAllocateDetails>();
            var allocateData = from allocate in allocates
                               from details in allocateDetails
                               where allocate.ID == details.BillID && productIDs.Contains(details.ProductID)
                               select new ProductQuantity
                               {
                                   Quantity = details.Quantity,
                                   ProductID = details.ProductID
                               };

            int status = (int)BillDeliveryStatusEnum.已装箱未配送;
            var deliveries = lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.Status == status);
            var deliveryDetails = lp.GetDataContext<BillDeliveryDetails>();
            var deliveryData = from delivery in deliveries
                               from details in deliveryDetails
                               where delivery.ID == details.BillID && productIDs.Contains(details.ProductID)
                               select new ProductQuantity
                               {
                                   Quantity = details.Quantity,
                                   ProductID = details.ProductID
                               };

            var deliveryResult = deliveryData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            var allocateResult = allocateData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();

            //占用库存
            allocateResult.AddRange(deliveryResult);
            return allocateResult;
        }
    }
}
