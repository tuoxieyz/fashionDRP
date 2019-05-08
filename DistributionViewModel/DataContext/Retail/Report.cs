using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using DistributionModel;
using DomainLogicEncap;
using DistributionViewModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class RetailSearchEntity : BillRetail
    {
        public DateTime CreateDate { get; set; }
        public string CreatorName { get; set; }
        public string OrganizationName { get; set; }
        public string StorageName { get; set; }
        public string VIPName { get; set; }
        public string VIPCode { get; set; }
        public string GuideName { get; set; }
        public string ShiftName { get; set; }
    }

    public class RetailEntityForAggregation : BillEntityForAggregation
    {
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal CostMoney { get; set; }
        /// <summary>
        /// 折后金额
        /// </summary>
        public decimal DiscountMoney { get; set; }
        /// <summary>
        /// 扣减金额
        /// </summary>
        public decimal CutMoney { get; set; }
        public string VIPCode { get; set; }
        public int? ShiftID { get; set; }
        public int? GuideID { get; set; }
    }

    public class RetailAggregationEntity : DistributionProductShow
    {
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal CostMoney { get; set; }
        /// <summary>
        /// 扣减金额
        /// </summary>
        public decimal CutMoney { get; set; }
        /// <summary>
        /// 折后金额
        /// </summary>
        public decimal DiscountMoney { get; set; }
    }

    public class RetailAchievementEntity : BillRetail
    {
        public int Year { get; set; }
        public string YearMonth { get; set; }
    }

    public class RetailEntityForDistribution : BillEntityForAggregation
    {
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal CostMoney { get; set; }
        /// <summary>
        /// 实收金额
        /// </summary>
        public decimal ReceiveMoney { get; set; }
        public decimal TicketMoney { get; set; }
    }

    public class RetailDistributionEntity
    {
        public string OrganizationName { get; set; }
        public int OrganizationID { get; set; }
        public DateTime CreateTime { get; set; }
        public int Year { get; set; }
        public string YearMonth { get; set; }
        public int Quantity { get; set; }
        public decimal CostMoney { get; set; }
        public decimal ReceiveMoney { get; set; }
        public decimal TicketMoney { get; set; }
        /// <summary>
        /// 应收金额 = 实收金额+抵扣金额
        /// </summary>
        public IEnumerable<decimal> CostMoneyRatio
        {
            get { return new decimal[] { ReceiveMoney, TicketMoney }; }
        }
    }

    public static partial class ReportDataContext
    {
        /// <summary>
        /// 零售单明细
        /// </summary>
        public static List<ProductForRetail> GetBillRetailDetails(int billID)
        {
            var detailsContext = _query.LinqOP.Search<BillRetailDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new ProductForRetail
                       {
                           ProductID = product.ProductID,
                           ProductCode = product.ProductCode,
                           BYQID = product.BYQID,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Price = details.Price,
                           CutMoney = details.CutMoney,
                           Quantity = details.Quantity,
                           Discount = details.Discount
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
        /// 零售单汇总
        /// </summary>
        internal static List<RetailAggregationEntity> AggregateBillRetail(IQueryable<RetailEntityForAggregation> data)
        {
            var temp = data.GroupBy(o => o.ProductID).Select(g => new
            {
                g.Key,
                Quantity = g.Sum(o => o.Quantity),
                DiscountMoney = g.Sum(o => o.DiscountMoney),
                CutMoney = g.Sum(o => o.CutMoney)
            }).ToList();
            var pids = temp.Select(o => o.Key).ToArray();
            var products = _query.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            var result = temp.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key);
                return new RetailAggregationEntity
                {
                    BYQID = product.BYQID,
                    ProductID = product.ProductID,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    SizeID = product.SizeID,
                    Quantity = o.Quantity,
                    DiscountMoney = o.DiscountMoney,
                    CutMoney = o.CutMoney,
                    StyleID = product.StyleID,
                    ColorID = product.ColorID,
                    Price = product.Price
                };
            }).ToList();
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
        /// 获取零售业绩
        /// <remarks>不涉及品牌过滤</remarks>
        /// </summary>
        public static IEnumerable<RetailAchievementEntity> GetSelfRetailAchievement(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var retailContext = lp.Search<BillRetail>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            retailContext = retailContext.Select(o => new BillRetail
            {
                CreateTime = o.CreateTime.Date,
                CostMoney = o.CostMoney,
                ReceiveMoney = o.ReceiveMoney,
                TicketMoney = o.TicketMoney,
                Quantity = o.Quantity
            });
            var data = (IQueryable<BillRetail>)retailContext.Where(filters);
            //return data.ToList();//CreateTime转换成Date之后虽然对filters有效，但获取的数据仍然是原来的带时分秒的CreateTime,只是在从数据库获取之后再转换为不带时分秒的Date值
            //目前groupby只能生成纯列名形式,如虽然是GroupBy(o => o.CreateTime.Date),但生成的sql语句仍然是groupby(CreateTime),而不是groupby(Convert(10,CreateTime,120))这种形式
            //var result = data.GroupBy(o => o.CreateTime).Select(g => new BillRetail
            //{
            //    CreateTime = g.Key,
            //    Quantity = g.Sum(o => o.Quantity),
            //    CostMoney = g.Sum(o => o.CostMoney),
            //    ReceiveMoney = g.Sum(o => o.ReceiveMoney),
            //    TicketMoney = g.Sum(o => o.TicketMoney)
            //}).ToList();
            //数据返回后再汇总可能会导致传输效率低下的问题
            var result = data.ToList().GroupBy(o => o.CreateTime).Select(g => new RetailAchievementEntity
            {
                CreateTime = g.Key,
                Year = g.Key.Year,
                YearMonth = g.Key.ToString("yyyy-MM"),
                Quantity = g.Sum(o => o.Quantity),
                CostMoney = g.Sum(o => o.CostMoney),
                ReceiveMoney = g.Sum(o => o.ReceiveMoney),
                TicketMoney = g.Sum(o => o.TicketMoney)
            }).OrderByDescending(o => o.CreateTime); ;
            return result;
        }
    }
}
