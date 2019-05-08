using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DBAccess;
using Telerik.Windows.Data;
using DistributionModel;
using DomainLogicEncap;
using DistributionViewModel;
using System.ComponentModel;
using SysProcessModel;
using System.Collections.ObjectModel;
using Model.Extension;
using ERPViewModelBasic;
using SysProcessViewModel;
using ViewModelBasic;
using System.Data;

namespace DistributionViewModel
{
    public static partial class ReportDataContext
    {
        private static QueryGlobal _query = VMGlobal.DistributionQuery;

        private static List<Storage> _storages;

        /// <summary>
        /// 用于报表查询的仓库信息
        /// </summary>
        /// <remarks>VMGlobal中的Storages表示状态为正常的仓库集合，在报表查询中需要显示所有包括已禁用的仓库</remarks>
        public static List<Storage> Storages
        {
            get
            {
                if (_storages == null)
                    _storages = _query.LinqOP.Search<Storage>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
                return _storages;
            }
        }

        private static FloatPriceHelper _fpHelper;
        private static FloatPriceHelper FPHelper
        {
            get
            {
                if (_fpHelper == null)
                {
                    _fpHelper = new FloatPriceHelper();
                }
                return _fpHelper;
            }
        }

        static ReportDataContext()
        {
            VMGlobal.RefreshingEvent += delegate { _storages = null; };
        }

        //private static List<ProBrand> _brands;

        ///// <summary>
        ///// 用于报表查询的品牌信息
        ///// </summary>
        ///// <remarks>同Storages</remarks>
        //public static List<ProBrand> Brands
        //{
        //    get
        //    {
        //        if (_brands == null)
        //            _brands = _query.LinqOP.Search<ProBrand>().ToList();
        //        return _brands;
        //    }
        //}

        /// <summary>
        /// 根据单据ID查询单据明细
        /// </summary>
        public static IEnumerable<DistributionProductShow> SearchBillDetails<TDetails>(int billID) where TDetails : BillDetailBase
        {
            var detailsContext = _query.LinqOP.Search<TDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new DistributionProductShow
                       {
                           ProductID = product.ProductID,
                           ProductCode = product.ProductCode,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Price = product.Price,
                           Quantity = details.Quantity,
                           BYQID = product.BYQID
                       };
            var result = data.ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                r.BrandID = byq.BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.Year = byq.Year;
                r.Quarter = byq.Quarter;
            }
            return result;
        }

        /// <summary>
        /// 单据汇总
        /// </summary>
        internal static List<DistributionProductShow> AggregateBill(IQueryable<IProductForAggregation> data, bool caluteFloatPrice = true)
        {
            var temp = data.GroupBy(o => o.ProductID).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            var pids = temp.Select(o => o.Key).ToArray();
            var products = _query.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = temp.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key);
                return new DistributionProductShow
                {
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    NameID = product.NameID,
                    Quantity = o.Quantity,
                    Price = product.Price,
                    BYQID = product.BYQID
                };
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                r.ProductName = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
                var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                r.BrandID = byq.BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.Year = byq.Year;
                r.Quarter = byq.Quarter;
            }
            if (caluteFloatPrice)
            {
                FloatPriceHelper fpHelper = new FloatPriceHelper();
                result.ForEach(o => o.Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.BYQID, o.Price));
            }
            return result;
        }

        private static void FilterBillWithBrand<TBill>(IQueryable<TBill> billContext, CompositeFilterDescriptorCollection condition, IEnumerable<int> brandIDs) where TBill : BillWithBrand
        {
            //if (!FilterConditionHelper.IsConditionSetted(condition, "BrandID"))
            billContext = billContext.Where(o => brandIDs.Contains(o.BrandID));
        }

        #region 订单查询

        /// <summary>
        /// 订单明细
        /// </summary>
        public static List<ProductForOrderReport> GetBillOrderDetails(int billID)
        {
            var detailsContext = _query.LinqOP.Search<BillOrderDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID && details.IsDeleted == false
                       select new ProductForOrderReport
                       {
                           ProductID = details.ProductID,
                           DetailID = details.ID,
                           ProductCode = product.ProductCode,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Price = product.Price,
                           Quantity = details.Quantity,
                           QuaCancel = details.QuaCancel,
                           QuaDelivered = details.QuaDelivered,
                           BYQID = product.BYQID
                       };
            var list = data.ToList();
            //list.ForEach(o =>
            //{
            //    var realQua = o.Quantity - o.QuaCancel;
            //    o.Status = realQua == o.QuaDelivered ? "已完成" : (o.QuaDelivered == 0 ? "未发货" : ((realQua > o.QuaDelivered ? "部分已发货" : "数据有误")));
            //});
            foreach (var d in list)
            {
                d.ColorCode = VMGlobal.Colors.Find(o => o.ID == d.ColorID).Code;
                var byq = VMGlobal.BYQs.Find(o => o.ID == d.BYQID);
                d.BrandID = byq.BrandID;
                d.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == d.BrandID).Code;
                d.SizeName = VMGlobal.Sizes.Find(o => o.ID == d.SizeID).Name;
                d.Year = byq.Year;
                d.Quarter = byq.Quarter;

                d.Price = FPHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, d.BYQID, d.Price);
            }
            return list;
        }

        /// <summary>
        /// 查询本级和下级订单
        /// </summary>
        public static List<OrderSearchEntity> SearchBillOrder(IEnumerable<IFilterDescriptor> billFilters, IEnumerable<IFilterDescriptor> detailsFilters, IEnumerable<int> oids, int pageIndex, int pageSize, ref int totalCount)
        {
            var lp = _query.LinqOP;
            var oidArray = oids.ToArray();
            var orderContext = lp.Search<BillOrder>(o => oidArray.Contains(o.OrganizationID));
            var userContext = lp.GetDataContext<ViewUser>();
            var orgContext = lp.Search<ViewOrganization>();

            var billData = from order in orderContext
                           from user in userContext
                           where order.CreatorID == user.ID
                           from org in orgContext
                           where org.ID == order.OrganizationID && org.Flag
                           select new OrderSearchEntity
                           {
                               BillID = order.ID,
                               BillCode = order.Code,
                               BrandID = order.BrandID,
                               OrganizationID = order.OrganizationID,
                               机构名称 = org.Name,
                               单据编号 = order.Code,
                               CreateDate = order.CreateTime.Date,
                               CreateTime = order.CreateTime,
                               开单人 = user.Name,
                               备注 = order.Remark,
                               订单状态 = !order.IsDeleted
                           };
            return SearchOrder(billData, billFilters, detailsFilters, pageIndex, pageSize, ref totalCount);
        }

        public static List<OrderSearchEntity> SearchOrder(IQueryable<OrderSearchEntity> billData, IEnumerable<IFilterDescriptor> billFilters, IEnumerable<IFilterDescriptor> detailsFilters, int pageIndex, int pageSize, ref int totalCount)
        {
            var lp = _query.LinqOP;
            var orderDetailsContext = lp.GetDataContext<BillOrderDetails>();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            billData = billData.Where(o => brandIDs.Contains(o.BrandID));
            if (FilterConditionHelper.IsConditionSetted(detailsFilters, "StyleCode"))
            {
                var productContext = lp.GetDataContext<ViewProduct>();
                var pdata = from p in productContext
                            where brandIDs.Contains(p.BrandID)
                            select new { ProductID = p.ProductID, StyleCode = p.StyleCode };
                IEnumerable<int> pIDs = ((IQueryable<dynamic>)pdata.Where(detailsFilters)).ToList().Select(p => (int)p.ProductID);
                if (pIDs.Count() == 0)
                    return null;
                else
                {
                    billData = from d in billData
                               where orderDetailsContext.Any(od => od.BillID == d.BillID && pIDs.Contains(od.ProductID))
                               select d;
                }
            }
            var filtedData = (IQueryable<OrderSearchEntity>)billData.Where(billFilters);
            totalCount = filtedData.Count();
            //var datas = new QueryableCollectionView(filtedData.OrderBy(o => o.BillID).Skip(pageIndex * pageSize).Take(pageSize).ToList()); //filtedData.ToList();
            var orders = filtedData.OrderBy(o => o.BillID).Skip(pageIndex * pageSize).Take(pageSize).ToList();//(List<OrderSearchEntity>)datas.SourceCollection;
            var bIDs = orders.Select(o => (int)o.BillID);
            var sum = orderDetailsContext.Where(o => bIDs.Contains(o.BillID) && o.IsDeleted == false).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, OrderQuantity = g.Sum(o => o.Quantity), CancelQuantity = g.Sum(o => o.QuaCancel), DeliveredQuantity = g.Sum(o => o.QuaDelivered) }).ToList();
            orders.ForEach(d =>
            {
                d.BrandName = brands.Find(o => d.BrandID == o.ID).Name;
                var order = sum.Find(o => o.BillID == d.BillID);
                if (order != null)
                {
                    d.订货数量 = order.OrderQuantity;
                    d.取消量 = order.CancelQuantity;
                    d.已发数量 = order.DeliveredQuantity;
                    var realOrderQuantity = d.订货数量 - d.取消量;
                    d.发货状态 = realOrderQuantity == d.已发数量 ? "已完成" : (d.已发数量 == 0 ? "未发货" : (realOrderQuantity > d.已发数量 ? "部分已发货" : "数据有误"));
                }
            });
            return orders;
        }

        /// <summary>
        /// 下级订单汇总
        /// </summary>
        internal static IQueryable<OrderEntityForAggregation> GetSubordinateOrderAggregation(IEnumerable<IFilterDescriptor> filters, IEnumerable<int> oids)
        {
            var lp = _query.LinqOP;

            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var orderContext = lp.Search<BillOrder>(o => brandIDs.Contains(o.BrandID));
            var orderDetailsContext = lp.GetDataContext<BillOrderDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            //var orgContext = lp.Search<ViewOrganization>(o => oids.Contains(o.ID));
            //假如未设置机构条件，则默认下级所有机构
            //if (!FilterConditionHelper.IsConditionSetted(filters, "OrganizationID"))
            //{
            //    var oids = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Select(o => o.ID).ToArray();
            //    orgContext = orgContext.Where(o => oids.Contains(o.ID));
            //}
            var data = from order in orderContext
                       from orderDetails in orderDetailsContext
                       where order.ID == orderDetails.BillID && oids.Contains(order.OrganizationID) && order.IsDeleted == false
                       from product in productContext
                       where product.ProductID == orderDetails.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new OrderEntityForAggregation
                       {
                           OrganizationID = order.OrganizationID,
                           BrandID = product.BrandID,
                           ProductID = product.ProductID,
                           CreateTime = order.CreateTime.Date,
                           NameID = product.NameID,
                           //ProductCode = product.ProductCode,
                           //BrandCode = product.BrandCode,
                           StyleCode = product.StyleCode,
                           //ColorCode = product.ColorCode,
                           //SizeName = product.SizeName,
                           //Price = product.Price,
                           Quantity = orderDetails.Quantity - orderDetails.QuaCancel,
                           QuaDelivered = orderDetails.QuaDelivered
                       };

            data = (IQueryable<OrderEntityForAggregation>)data.Where(filters);
            return data;
        }

        public static IEnumerable<BillSnapshot> GetUniqueCodeTransTrack(string uniqueCode)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var snapshots = lp.GetDataContext<BillSnapshot>();
            var snapshotDetails = lp.Search<BillSnapshotDetailsWithUniqueCode>(o => o.UniqueCode == uniqueCode);
            var data = (from details in snapshotDetails
                        from snapshot in snapshots
                        where snapshot.ID == details.SnapshotID
                        select snapshot).ToList();
            return data.OrderBy(o => o.CreateTime);
        }

        #endregion
    }
}
