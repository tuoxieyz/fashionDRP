using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class ShopGuiderSaleAchievementVM : CommonViewModel<ShopGuiderSaleAchievementEntity>
    {
        public int BrandID { get; set; }

        private DateTime _beginDate = DateTime.Now.AddMonths(-1).Date;
        public DateTime BeginDate { get { return _beginDate; } set { _beginDate = value; } }

        private DateTime _endDate = DateTime.Now.Date;
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; } }

        public ShopGuiderSaleAchievementVM()
        {
            if (VMGlobal.PoweredBrands.Count == 1)
                BrandID = VMGlobal.PoweredBrands.First().ID;
        }

        protected IEnumerable<ShopGuiderSaleAchievementEntity> SearchData(IQueryable<BillRetail> retailContext)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;

            //var retailContext = lp.Search<BillRetail>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.CreateTime >= BeginDate && o.CreateTime <= endDate);
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var guiderContext = lp.GetDataContext<RetailShoppingGuide>();
            var shifts = lp.GetDataContext<RetailShift>();
            var data = from retail in retailContext
                       from guider in guiderContext
                       where retail.GuideID == guider.ID
                       from shift in shifts
                       where shift.ID == guider.ShiftID
                       from details in detailsContext
                       where retail.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID && product.BrandID == BrandID
                       select new
                       {
                           ShiftName = shift.Name,
                           GuiderCode = guider.Code,
                           GuiderName = guider.Name,
                           Quantity = details.Quantity,
                           Pirce = details.Price,
                           Discount = details.Discount,
                           CutMoney = details.CutMoney,
                           OrganizationID = retail.OrganizationID
                       };
            var temp = data.ToList();
            var result = temp.GroupBy(o => new { o.GuiderCode, o.GuiderName, o.ShiftName, o.OrganizationID }).Select(g => new ShopGuiderSaleAchievementEntity
            {
                ShiftName = g.Key.ShiftName,
                GuiderName = g.Key.GuiderName,
                GuiderCode = g.Key.GuiderCode,
                SalePrice = g.Sum(o => o.Quantity > 0 ? o.Quantity * o.Pirce : 0),
                SaleMoney = g.Sum(o => o.Quantity > 0 ? o.Quantity * o.Pirce * o.Discount / 100 - o.CutMoney : 0),
                SaleQuantity = g.Sum(o => o.Quantity > 0 ? o.Quantity : 0),
                GRPrice = g.Sum(o => o.Quantity < 0 ? o.Quantity * o.Pirce : 0),
                GRMoney = g.Sum(o => o.Quantity < 0 ? o.Quantity * o.Pirce * o.Discount / 100 - o.CutMoney : 0),
                GRQuantity = g.Sum(o => o.Quantity < 0 ? o.Quantity : 0),
                OrganizationID = g.Key.OrganizationID
            }).ToList();
            foreach (var r in result)
            {
                r.ResultPrice = r.SalePrice + r.GRPrice;
                r.ResultMoney = r.SaleMoney + r.GRMoney;
                r.ResultQuantity = r.SaleQuantity + r.GRQuantity;
                if (r.ResultPrice != 0)
                    r.Discount = Math.Round(r.ResultMoney / r.ResultPrice, 4);
            }
            return result;
        }

        protected override IEnumerable<ShopGuiderSaleAchievementEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var endDate = EndDate.AddDays(1);
            var retailContext = lp.Search<BillRetail>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.CreateTime >= BeginDate && o.CreateTime <= endDate);
            return this.SearchData(retailContext);
        }
    }

    public class ShopGuiderSaleAchievementEntity
    {
        public string ShiftName { get; set; }
        public string GuiderCode { get; set; }
        public string GuiderName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal SaleMoney { get; set; }
        public int SaleQuantity { get; set; }
        public decimal GRPrice { get; set; }
        public decimal GRMoney { get; set; }
        public int GRQuantity { get; set; }
        public decimal ResultPrice { get; set; }
        public decimal ResultMoney { get; set; }
        public int ResultQuantity { get; set; }
        public decimal? Discount { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; }
    }
}
