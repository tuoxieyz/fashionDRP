using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace DistributionViewModel
{
    public class RetailABCAnalysisVM : ViewModelBase
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public Visibility OrganizationSeletorVisibility { get; set; }

        public int BrandID { get; set; }

        public int Year { get; set; }

        public int Quarter { get; set; }

        private DateTime _beginDate = DateTime.Now.AddMonths(-1).Date;
        public DateTime BeginDate { get { return _beginDate; } set { _beginDate = value; } }

        private DateTime _endDate = DateTime.Now.Date;
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; } }

        public IEnumerable<ABCEntity> StyleABCEntity { get; set; }
        public IEnumerable<ABCEntity> ColorABCEntity { get; set; }
        public IEnumerable<ABCEntity> ProNameABCEntity { get; set; }
        public decimal AmountCostMoney { get; set; }

        //public ICommand SearchCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(param =>
        //        {
        //            this.SearchData();
        //        });
        //    }
        //}

        public RetailABCAnalysisVM()
        {
            Year = DateTime.Now.Year;
            if (VMGlobal.PoweredBrands.Count == 1)
                BrandID = VMGlobal.PoweredBrands.First().ID;
            if (OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Count == 0)
                OrganizationSeletorVisibility = Visibility.Collapsed;
            else
                OrganizationSeletorVisibility = Visibility.Visible;
        }

        public void SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var endDate = EndDate.AddDays(1);
            var retailContext = lp.Search<BillRetail>(o => o.CreateTime >= BeginDate && o.CreateTime <= endDate && oids.Contains(o.OrganizationID));
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            if (BrandID != default(int))
                productContext = productContext.Where(p => p.BrandID == BrandID);
            if (Quarter != default(int))
                productContext = productContext.Where(p => p.Quarter == Quarter);
           
            var data = from retail in retailContext
                       from details in detailsContext
                       where retail.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID && product.Year == Year
                       select new ABCEntity
                       {
                           StyleCode = product.StyleCode,
                           CostMoney = details.Price * details.Quantity * details.Discount / 100 - details.CutMoney,
                           ColorID = product.ColorID,
                           NameID = product.NameID
                       };
            var result = data.ToList();
            foreach (var r in result)
            {
                r.Name = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
            }
            AmountCostMoney = result.Sum(o => o.CostMoney);
            StyleABCEntity = result.GroupBy(o => new { o.StyleCode, o.Name }).Select(g => new ABCEntity
            {
                StyleCode = g.Key.StyleCode,
                Name = g.Key.Name,
                CostMoney = g.Sum(o => o.CostMoney)
            }).OrderByDescending(o => o.CostMoney).ToList();
            decimal accumulativeProportion = 0;
            foreach (var abc in StyleABCEntity)
            {
                abc.Proportion = abc.CostMoney / AmountCostMoney;
                accumulativeProportion += abc.Proportion;
                abc.AccuProportion = accumulativeProportion;
            }
            ProNameABCEntity = result.GroupBy(o => o.Name).Select(g => new ABCEntity
            {
                Name = g.Key,
                CostMoney = g.Sum(o => o.CostMoney)
            }).OrderByDescending(o => o.CostMoney).ToList();
            accumulativeProportion = 0;
            foreach (var abc in ProNameABCEntity)
            {
                abc.Proportion = abc.CostMoney / AmountCostMoney;
                accumulativeProportion += abc.Proportion;
                abc.AccuProportion = accumulativeProportion;
            }
            ColorABCEntity = result.GroupBy(o => o.ColorID).Select(g => new ABCEntity
            {
                ColorID = g.Key,
                CostMoney = g.Sum(o => o.CostMoney)
            }).OrderByDescending(o => o.CostMoney).ToList();
            accumulativeProportion = 0;
            foreach (var abc in ColorABCEntity)
            {
                abc.ColorName = VMGlobal.Colors.Find(o => o.ID == abc.ColorID).Name;
                abc.Proportion = abc.CostMoney / AmountCostMoney;
                accumulativeProportion += abc.Proportion;
                abc.AccuProportion = accumulativeProportion;
            }
            OnPropertyChanged("StyleABCEntity");
            OnPropertyChanged("ColorABCEntity");
            OnPropertyChanged("ProNameABCEntity");
            OnPropertyChanged("AmountCostMoney");
        }
    }
}
