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
    public class SubordinateRetailDayReportVM : CommonViewModel<RetailDayReportEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public int BrandID { get; set; }

        private DateTime _retailDay = DateTime.Now.Date;
        public DateTime RetailDay { get { return _retailDay; } set { _retailDay = value; } }

        public SubordinateRetailDayReportVM()
        {
            if (VMGlobal.PoweredBrands.Count == 1)
                BrandID = VMGlobal.PoweredBrands.First().ID;
        }

        protected override IEnumerable<RetailDayReportEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var retailContext = lp.Search<BillRetail>(o => o.CreateTime.Month == RetailDay.Month && oids.Contains(o.OrganizationID));
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();

            var data = from retail in retailContext
                       from details in detailsContext
                       where retail.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID && product.BrandID == BrandID
                       select new
                       {
                           retail.OrganizationID,
                           details.Quantity,
                           details.Discount,
                           details.CutMoney,
                           details.Price,
                           CreateDay = retail.CreateTime.Day
                       };
            var result = data.GroupBy(o => o.OrganizationID).Select(g => new RetailDayReportEntity
            {
                OrganizationID = g.Key,
                Quantity = g.Sum(o => o.CreateDay == RetailDay.Day ? o.Quantity : 0),
                SaleMoney = g.Sum(o => o.CreateDay == RetailDay.Day ? o.Quantity * o.Price * o.Discount / 100 - o.CutMoney : 0),
                SalePrice = g.Sum(o => o.CreateDay == RetailDay.Day ? o.Quantity * o.Price : 0),
                MonthQuantity = g.Sum(o => o.Quantity),
                MonthSaleMoney = g.Sum(o => o.Quantity * o.Price * o.Discount / 100 - o.CutMoney)
            }).ToList();
            var targets = lp.Search<RetailMonthTaget>(o => o.Year == RetailDay.Year && o.Month == RetailDay.Month && oids.Contains(o.OrganizationID)).ToList();
            foreach (var r in result)
            {
                r.OrganizationCode = OrganizationArray.First(o => o.ID == r.OrganizationID).Code;
                r.OrganizationName = OrganizationArray.First(o => o.ID == r.OrganizationID).Name;
                if (r.SalePrice != 0)
                    r.Discount = r.SaleMoney / r.SalePrice;
                var target = targets.FirstOrDefault(o => o.OrganizationID == r.OrganizationID && o.Year == RetailDay.Year && o.Month == RetailDay.Month);
                if (target != null && target.SaleTaget != 0)
                {
                    r.MonthTarget = target.SaleTaget;
                    r.DayTarget = r.MonthTarget / DateTime.DaysInMonth(RetailDay.Year, RetailDay.Month);
                    r.CompletionRate = r.SaleMoney / r.DayTarget;
                    r.MonthCompletionRate = r.MonthSaleMoney / r.MonthTarget;
                    r.MonthUndone = r.MonthTarget - r.MonthSaleMoney;
                }
            }
            return result;
        }
    }

    public class RetailDayReportEntity
    {
        public int OrganizationID { get; set; }
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; }
        public int Quantity { get; set; }
        public decimal SaleMoney { get; set; }//业绩
        public decimal SalePrice { get; set; }//吊牌价
        public decimal Discount { get; set; }
        public decimal DayTarget { get; set; }
        public decimal CompletionRate { get; set; }
        public int MonthQuantity { get; set; }
        public decimal MonthSaleMoney { get; set; }
        public decimal MonthTarget { get; set; }
        public decimal MonthCompletionRate { get; set; }
        public decimal MonthUndone { get; set; }
    }
}
