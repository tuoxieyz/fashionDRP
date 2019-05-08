using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionViewModel;
using DistributionModel;
using Telerik.Windows.Data;
using System.Collections;
using System.Transactions;
using Kernel;
using Model.Extension;
using ERPViewModelBasic;
using SysProcessViewModel;
using SysProcessModel;
using ERPModelBO;

namespace DistributionViewModel
{
    public class ProductForStoreMove : DistributionProductShow
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

        private int _inStorageStock;
        /// <summary>
        /// 移入仓库库存
        /// </summary>
        public int InStorageStock
        {
            get { return _inStorageStock; }
            set
            {
                if (_inStorageStock != value)
                {
                    _inStorageStock = value;
                    OnPropertyChanged("InStorageStock");
                }
            }
        }
    }

    public class StoreMoveSearchEntity : BillStoreMove
    {
        public string BrandName { get; set; }
        public string CreatorName { get; set; }
        public string OutStorageName { get; set; }
        public string InStorageName { get; set; }
        public int Quantity { get; set; }
    }

    public class StoreMoveEntityForAggregation : BillStoreMove, IProductForAggregation
    {
        public int ProductID { get; set; }
        public string StyleCode { get; set; }
        public int Quantity { get; set; }
    }

    public class StoreMoveAggregationEntity : DistributionProductShow
    {
        public string OutStorageName { get; set; }
        public string InStorageName { get; set; }
    }

    public class BillStoreMoveVM : DistributionBillVM<BillStoreMove, BillStoreMoveDetails, ProductForStoreMove>
    {
        public OPResult ValidateWhenSave()
        {
            if (Master.StorageIDOut == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定移出仓库" };
            }
            if (Master.StorageIDIn == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定移入仓库" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定移货品牌" };
            }
            if (Master.StorageIDOut == Master.StorageIDIn)
            {
                return new OPResult { IsSucceed = false, Message = "移入仓库和移出仓库不能是同一个仓库" };
            }
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            var bo = new BillBO<BillStoreMove, BillStoreMoveDetails>
            {
                Bill = this.Master,
                Details = this.Details
            };
            var result = BillWebApiInvoker.Instance.SaveBill<BillStoreMove, BillStoreMoveDetails>(bo);
            if (result.IsSucceed)
                this.Init();
            return result;
        }

        //    protected override List<DistributionTempProductForBrush> GetTempProductForBill(string code)
        //    {
        //        var brandIDs = ProductContext.Brands.Select(o => o.ID);
        //        var products = LinqOP.GetDataContext<Product>();
        //        var prostyles = LinqOP.GetDataContext<ProStyle>();
        //        var byqs = LinqOP.GetDataContext<ProBYQ>();
        //        var query = from s in prostyles
        //                    from byq in byqs
        //                    where s.BYQID == byq.ID && brandIDs.Contains(byq.BrandID)
        //                    from p in products
        //                    where p.StyleID == s.ID && (p.Code == code || s.Code == code) && p.Flag
        //                    select new DistributionTempProductForBrush
        //                    {
        //                        ProductID = p.ID,
        //                        ProductCode = p.Code,
        //                        BrandID = byq.BrandID,
        //                        StyleCode = s.Code,
        //                        ColorID = p.ColorID,
        //                        SizeID = p.SizeID,
        //                        Year = byq.Year,
        //                        Quarter = byq.Quarter,
        //                        Price = s.Price,
        //                        BYQID = s.BYQID
        //                    };
        //        return query.ToList();
        //    }

        //    public override List<ProductForStoreMove> GetProductForBill(string code)
        //    {
        //        ProductDataContext productContext = this.ProductContext;
        //        var ps = this.GetTempProductForBill(code);

        //        if (ps.Count > 0)
        //        {
        //            return ps.Select(o => new ProductForStoreMove
        //            {
        //                ProductID = o.ProductID,
        //                ProductCode = o.ProductCode,
        //                BrandCode = productContext.Brands.FirstOrDefault(b => b.ID == o.BrandID).Code,
        //                StyleCode = o.StyleCode,
        //                ColorCode = productContext.Colors.FirstOrDefault(c => c.ID == o.ColorID).Code,
        //                SizeCode = productContext.Sizes.FirstOrDefault(s => s.ID == o.SizeID).Code,
        //                SizeName = productContext.Sizes.FirstOrDefault(s => s.ID == o.SizeID).Name,
        //                Price = FPHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.BrandID, o.Year, o.Quarter, o.Price),
        //            }).ToList();
        //        }
        //        return null;
        //    }
    }

    public static partial class ReportDataContext
    {
        public static List<StoreMoveAggregationEntity> AggregateStoreMove(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var storemoveContext = lp.Search<BillStoreMove>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            FilterBillWithBrand(storemoveContext, filters, brandIDs);
            var detailsContext = lp.GetDataContext<BillStoreMoveDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from sm in storemoveContext
                       from details in detailsContext
                       where sm.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new StoreMoveEntityForAggregation
                       {
                           ID = sm.ID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           Code = sm.Code,
                           CreateTime = sm.CreateTime.Date,
                           StorageIDIn = sm.StorageIDIn,
                           StorageIDOut = sm.StorageIDOut,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity
                       };
            var filtedData = (IQueryable<StoreMoveEntityForAggregation>)data.Where(filters);
            var sum = filtedData.GroupBy(o => new { o.ProductID, o.StorageIDOut, o.StorageIDIn }).Select(g => new
            {
                Key = g.Key,
                Quantity = g.Sum(o => o.Quantity)
            }).ToList();
            var pids = sum.Select(o => o.Key.ProductID).ToArray();
            var products = _query.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = sum.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key.ProductID);
                return new StoreMoveAggregationEntity
                {
                    BYQID = product.BYQID,
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    OutStorageName = Storages.Find(s => s.ID == o.Key.StorageIDOut).Name,
                    InStorageName = Storages.Find(s => s.ID == o.Key.StorageIDIn).Name
                };
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.BrandID = VMGlobal.BYQs.Find(o => o.ID == r.BYQID).BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
            }
            return result;
        }
    }
}
