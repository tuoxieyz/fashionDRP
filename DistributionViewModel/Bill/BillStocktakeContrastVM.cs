using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using DistributionModel;
using System.Collections;
using DistributionViewModel;
using DomainLogicEncap;
using ERPViewModelBasic;
using SysProcessModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class ContrastSearchEntity : BillStocktakeContrast
    {
        public string OrganizationName { get; set; }
        public string BrandName { get; set; }
        /// <summary>
        /// 更新人姓名
        /// </summary>
        public string CreatorName { get; set; }
        /// <summary>
        /// 更新仓库
        /// </summary>
        public string StorageName { get; set; }
        public int Quantity { get; set; }
        public int QuaStocktake { get; set; }
        public int QuaStockOrig { get; set; }
        /// <summary>
        /// 差异数量(不同于盈亏数量，差异数量为各条码盈亏数量绝对值的总和)
        /// </summary>
        public int QuaContrast { get; set; }
    }

    public class ContrastDetailsSearchEntity : DistributionProductShow
    {
        public int QuaStocktake { get; set; }
        public int QuaStockOrig { get; set; }
        /// <summary>
        /// 差异数量(不同于盈亏数量，差异数量为各条码盈亏数量绝对值的总和)
        /// </summary>
        public int QuaContrast { get; set; }
    }

    public class ContrastEntityForAggregation : BillStocktakeContrast, IProductForAggregation
    {
        public int ProductID { get; set; }
        public string StyleCode { get; set; }
        public int Quantity { get; set; }
        public int QuaStocktake { get; set; }
        public int QuaStockOrig { get; set; }
    }

    public static partial class ReportDataContext
    {
        public static List<ContrastDetailsSearchEntity> GetBillStocktakeContrastDetails(int billID)
        {
            var detailsContext = _query.LinqOP.Search<BillStocktakeContrastDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new ContrastDetailsSearchEntity
                       {
                           ProductCode = product.ProductCode,
                           BYQID = product.BYQID,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Quantity = details.Quantity,
                           QuaStockOrig = details.QuaStockOrig,
                           QuaStocktake = details.QuaStocktake
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

        public static List<ContrastDetailsSearchEntity> AggregateStocktakeContrast(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var contrastContext = lp.Search<BillStocktakeContrast>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            FilterBillWithBrand(contrastContext, filters, brandIDs);
            var detailsContext = lp.GetDataContext<BillStocktakeContrastDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from st in contrastContext
                       from details in detailsContext
                       where st.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new ContrastEntityForAggregation
                       {
                           ID = st.ID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           Code = st.Code,
                           CreateTime = st.CreateTime.Date,
                           StorageID = st.StorageID,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           QuaStocktake = details.QuaStocktake,
                           QuaStockOrig = details.QuaStockOrig
                       };
            var filtedData = (IQueryable<ContrastEntityForAggregation>)data.Where(filters);
            var sum = filtedData.GroupBy(o => o.ProductID).Select(g => new
            {
                ProductID = g.Key,
                Quantity = g.Sum(o => o.Quantity),
                QuaStocktake = g.Sum(o => o.QuaStocktake),
                QuaStockOrig = g.Sum(o => o.QuaStockOrig),
                QuaContrast = g.Sum(o => Math.Abs(o.QuaStocktake - o.QuaStockOrig))
            }).ToList();
            var pids = sum.Select(o => o.ProductID).ToArray();
            var products = _query.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = sum.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.ProductID);
                return new ContrastDetailsSearchEntity
                {
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    BYQID = product.BYQID,
                    StyleCode = product.StyleCode,
                    ColorID = product.ColorID,     
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    QuaStocktake = o.QuaStocktake,
                    QuaStockOrig = o.QuaStockOrig,
                    QuaContrast = o.QuaContrast
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

        public static List<ContrastDetailsSearchEntity> AggregateSubordinateStocktakeContrast(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = childOrganizations.Select(o => o.ID);
            var contrastContext = lp.Search<BillStocktakeContrast>(o => oids.Contains(o.OrganizationID));
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            FilterBillWithBrand(contrastContext, filters, brandIDs);
            var detailsContext = lp.GetDataContext<BillStocktakeContrastDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from st in contrastContext
                       from details in detailsContext
                       where st.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new ContrastEntityForAggregation
                       {
                           ID = st.ID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           Code = st.Code,
                           CreateTime = st.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           QuaStocktake = details.QuaStocktake,
                           QuaStockOrig = details.QuaStockOrig,
                           OrganizationID = st.OrganizationID
                       };
            var filtedData = (IQueryable<ContrastEntityForAggregation>)data.Where(filters);
            var sum = filtedData.GroupBy(o => o.ProductID).Select(g => new
            {
                ProductID = g.Key,
                Quantity = g.Sum(o => o.Quantity),
                QuaStocktake = g.Sum(o => o.QuaStocktake),
                QuaStockOrig = g.Sum(o => o.QuaStockOrig),
                QuaContrast = g.Sum(o => Math.Abs(o.QuaStocktake - o.QuaStockOrig))
            }).ToList();
            var pids = sum.Select(o => o.ProductID).ToArray();
            var products = _query.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = sum.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.ProductID);
                return new ContrastDetailsSearchEntity
                {                    
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,                    
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    QuaStocktake = o.QuaStocktake,
                    QuaStockOrig = o.QuaStockOrig,
                    QuaContrast = o.QuaContrast
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
