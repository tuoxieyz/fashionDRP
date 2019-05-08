using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using DistributionModel;
using Kernel;
using System.Transactions;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class MonthSaleTargetBatchSetVM : CommonViewModel<RetailMonthTagetBO>
    {
        private int _year;
        private int _month;

        public DateTime? YearMonth
        {
            get
            {
                if (_year < 1 || _year > 9999 || _month < 1 || _month > 12)
                    return null;
                return new DateTime(_year, _month, 1);
            }
            set
            {
                if (value != null)
                {
                    _year = value.Value.Year;
                    _month = value.Value.Month;
                    Entities = this.SearchData();
                }
                else
                {
                    _year = _month = 0;
                    Entities = null;
                }
            }
        }

        protected override IEnumerable<RetailMonthTagetBO> SearchData()
        {
            var organizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = organizations.Select(o => o.ID);
            var targets = VMGlobal.DistributionQuery.LinqOP.Search<RetailMonthTaget>(o => oids.Contains(o.OrganizationID) && _year == o.Year && _month == o.Month).ToList();
            return organizations.Select(o =>
            {
                var item = new RetailMonthTagetBO
                {
                    Year = _year,
                    Month = _month,
                    OrganizationCode = o.Code,
                    OrganizationName = o.Name,
                    OrganizationID = o.ID
                };
                var target = targets.Find(t => t.OrganizationID == o.ID);
                item.SaleTaget = target == null ? 0 : target.SaleTaget;
                item.ID = target == null ? default(int) : target.ID;
                return item;
            }).OrderBy(o => o.OrganizationName).ToList();
        }

        public OPResult Save()
        {
            if (Entities == null || Entities.Count() == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有可供保存的数据." };
            }
            var todeletes = Entities.Where(o => o.SaleTaget == 0 && o.ID != default(int));
            var toau = Entities.Where(o => o.SaleTaget != 0);
            foreach (var au in toau)
            {
                if (au.ID == default(int))
                {
                    au.CreatorID = VMGlobal.CurrentUser.ID;
                    au.CreateTime = DateTime.Now;
                }
            }
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    VMGlobal.DistributionQuery.LinqOP.Delete<RetailMonthTaget>(todeletes);//删除0指标数据
                    VMGlobal.DistributionQuery.LinqOP.AddOrUpdate<RetailMonthTaget>(toau);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "保存成功." };
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n." + e.Message };
                }
            }
        }
    }
}
