using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using DistributionModel;
using SysProcessModel;

namespace DomainLogicEncap
{
    public static class ProductLogic
    {
        private static QueryGlobal SysProcessQuery { get; set; }
        private static LinqOPEncap SysProcessLinqOP { get; set; }

        static ProductLogic()
        {
            SysProcessQuery = new QueryGlobal("SysProcessConstr");
            SysProcessLinqOP = SysProcessQuery.LinqOP;
        }

        public static ProBYQ GetBYQ(int brandID, int year, int quarter)
        {
            var byq = SysProcessLinqOP.Search<ProBYQ>(o => o.BrandID == brandID && o.Year == year && o.Quarter == quarter).FirstOrDefault();
            return byq;
        }

        public static IEnumerable<ProStyle> GetProStyles(int brandID, int year = 0, int quarter = 0, string styleCode = "")
        {
            var byqs = SysProcessLinqOP.Search<ProBYQ>();
            if (brandID != default(int))
                byqs = byqs.Where(o => o.BrandID == brandID);
            if (year != default(int))
                byqs = byqs.Where(o => o.Year == year);
            if (quarter != default(int))
                byqs = byqs.Where(o => o.Quarter == quarter);
            var styles = SysProcessLinqOP.Search<ProStyle>();
            if (!string.IsNullOrEmpty(styleCode))
                styles = styles.Where(o => o.Code.Contains(styleCode));
            var data = from style in styles
                       from byq in byqs
                       where byq.ID == style.BYQID
                       select style;
            return data.ToList();
        }
    }
}
