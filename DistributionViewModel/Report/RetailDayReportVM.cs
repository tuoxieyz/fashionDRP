using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;

namespace DistributionViewModel
{
    public class RetailDayReportVM : ViewModelBase
    {
        public int BrandID { get; set; }

        private DateTime _retailDay = DateTime.Now.Date;
        public DateTime RetailDay { get { return _retailDay; } set { _retailDay = value; } }

        public RetailDayReportEntity Entity { get; set; }
        public List<ABCEntity> StyleABCEntities { get; set; }
        public List<ShopGuiderSaleAchievementEntity> GuideEntities { get; set; }
        public List<PieABCEntity> ProNameEntities { get; set; }

        public RetailDayReportVM()
        {
            if (VMGlobal.PoweredBrands.Count == 1)
                BrandID = VMGlobal.PoweredBrands.First().ID;
        }

        public void SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var retailContext = lp.Search<BillRetail>(o => o.CreateTime.Month == RetailDay.Month && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from retail in retailContext
                       from details in detailsContext
                       where retail.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID && product.BrandID == BrandID
                       select new
                       {
                           product.StyleCode,
                           product.NameID,
                           retail.GuideID,
                           retail.VIPID,
                           details.Quantity,
                           details.Discount,
                           details.CutMoney,
                           details.Price,
                           CreateDay = retail.CreateTime.Day
                       };
            var temp = data.ToList();
            var target = lp.Search<RetailMonthTaget>(o => o.Year == RetailDay.Year && o.Month == RetailDay.Month && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).FirstOrDefault();
            Entity = new RetailDayReportEntity
            {
                Quantity = temp.Sum(o => o.CreateDay == RetailDay.Day ? o.Quantity : 0),
                SaleMoney = temp.Sum(o => o.CreateDay == RetailDay.Day ? o.Quantity * o.Price * o.Discount / 100 - o.CutMoney : 0),
                SalePrice = temp.Sum(o => o.CreateDay == RetailDay.Day ? o.Quantity * o.Price : 0),
                MonthQuantity = temp.Sum(o => o.Quantity),
                MonthSaleMoney = temp.Sum(o => o.Quantity * o.Price * o.Discount / 100 - o.CutMoney)
            };
            if (Entity.SalePrice != 0)
                Entity.Discount = Entity.SaleMoney / Entity.SalePrice;
            if (target != null && target.SaleTaget != 0)
            {
                Entity.MonthTarget = target.SaleTaget;
                Entity.DayTarget = Entity.MonthTarget / DateTime.DaysInMonth(RetailDay.Year, RetailDay.Month);
                Entity.CompletionRate = Entity.SaleMoney / Entity.DayTarget;
                Entity.MonthCompletionRate = Entity.MonthSaleMoney / Entity.MonthTarget;
                Entity.MonthUndone = Entity.MonthTarget - Entity.MonthSaleMoney;
            }

            var dayData = temp.Where(o => o.CreateDay == RetailDay.Day).ToArray();
            StyleABCEntities = dayData.GroupBy(o => o.StyleCode).Select(g => new ABCEntity
            {
                StyleCode = g.Key,
                CostMoney = g.Sum(o => o.Quantity * o.Price * o.Discount / 100 - o.CutMoney),
                Quantity = g.Sum(o => o.Quantity),
            }).OrderByDescending(o => o.CostMoney).ToList();
            var guides = lp.Search<RetailShoppingGuide>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            GuideEntities = dayData.GroupBy(o => o.GuideID).Select(g =>
            {
                var guideName = g.Key == null ? "顾客自选" : guides.Find(o => o.ID == g.Key).Name;
                return new ShopGuiderSaleAchievementEntity
                 {
                     GuiderName = guideName,
                     SaleMoney = g.Sum(o => o.Quantity * o.Price * o.Discount / 100),
                     SaleQuantity = g.Sum(o => o.Quantity)
                 };
            }).ToList();
            var amount = GuideEntities.Sum(o => o.SaleMoney);
            foreach (var g in GuideEntities)
            {
                g.Title = string.Format("{0}: {1:C}; {2}件", g.GuiderName, g.SaleMoney, g.SaleQuantity);
                if (amount != 0)
                    g.Description = string.Format("{0}%", Math.Round(g.SaleMoney * 100 / amount));
            }
            ProNameEntities = dayData.GroupBy(o => o.NameID).Select(g => new PieABCEntity
                {
                    Name = VMGlobal.ProNames.Find(o => o.ID == g.Key).Name,
                    CostMoney = g.Sum(o => o.Quantity * o.Price * o.Discount / 100),
                    Quantity = g.Sum(o => o.Quantity)
                }).ToList();
            amount = ProNameEntities.Sum(o => o.CostMoney);
            foreach (var g in ProNameEntities)
            {
                g.Title = string.Format("{0}: {1:C}; {2}件", g.Name, g.CostMoney, g.Quantity);
                if (amount != 0)
                    g.Description = string.Format("{0}%", Math.Round(g.CostMoney * 100 / amount));
            }

            OnPropertyChanged("Entity");
            OnPropertyChanged("StyleABCEntities");
            OnPropertyChanged("GuideEntities");
            OnPropertyChanged("ProNameEntities");
        }
    }

    public class PieABCEntity:ABCEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
