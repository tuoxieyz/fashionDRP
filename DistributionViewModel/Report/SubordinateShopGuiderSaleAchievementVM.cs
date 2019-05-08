using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class SubordinateShopGuiderSaleAchievementVM : ShopGuiderSaleAchievementVM
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        protected override IEnumerable<ShopGuiderSaleAchievementEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var endDate = EndDate.AddDays(1);
            var retailContext = lp.Search<BillRetail>(o => oids.Contains(o.OrganizationID) && o.CreateTime >= BeginDate && o.CreateTime <= endDate);
            var result = this.SearchData(retailContext);
            foreach (var r in result)
            {
                var org = OrganizationArray.FirstOrDefault(o => o.ID == r.OrganizationID);
                if (org != null)
                {
                    r.OrganizationName = org.Name;
                }
            }
            return result;
        }
    }
}
