using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using System.Data;
using DistributionModel;
using SysProcessModel;

namespace DomainLogicEncap
{
    /// <summary>
    /// 上浮价格辅助类
    /// </summary>
    public class FloatPriceHelper
    {
        private QueryGlobal _query = new QueryGlobal("SysProcessConstr");

        #region 辅助类

        private class PriceFloatCache
        {
            public int OrganizationID { get; set; }
            //public ProBYQ BYQ { get; set; }
            public int BYQID { get; set; }
            public List<PriceFloatItem> PriceFloatItems { get; set; }
        }

        private class PriceFloatItem
        {
            public decimal FloatRate { get; set; }
            public int LastNumber { get; set; }
        }

        #endregion

        private List<PriceFloatCache> _priceFloatCache = new List<PriceFloatCache>();

        public decimal GetFloatPrice(int organizationID, int byqID, decimal price)
        {
            var pf = _priceFloatCache.FirstOrDefault(o => o.OrganizationID == organizationID && o.BYQID == byqID);
            if (pf == null)
            {
                var ds = _query.DB.ExecuteDataSet("GetOrganizationPriceHierarchy", organizationID, byqID);
                pf = new PriceFloatCache { BYQID = byqID, OrganizationID = organizationID };
                _priceFloatCache.Add(pf);
                var table = ds.Tables[0];
                if (table.Rows.Count > 0)
                {
                    pf.PriceFloatItems = new List<PriceFloatItem>();
                    foreach (DataRow row in table.Rows)
                    {
                        pf.PriceFloatItems.Add(new PriceFloatItem { FloatRate = (decimal)row["FloatRate"], LastNumber = (int)row["LastNumber"] });
                    }
                }
            }
            if (pf.PriceFloatItems != null && pf.PriceFloatItems.Count > 0)
            {
                pf.PriceFloatItems.ForEach(o =>
                {
                    price += o.FloatRate * price * 0.01M;//上浮
                    price *= 0.1M;
                    price = decimal.Truncate(price) * 10 + o.LastNumber;//尾数
                });
            }
            return price;
        }

        public decimal GetFloatPrice(int organizationID, int productID)
        {
            var lp = _query.LinqOP;
            var product = lp.GetDataContext<Product>();
            var proStyle = lp.GetDataContext<ProStyle>();
            var dataQuery = from p in product
                            from s in proStyle
                            where p.StyleID == s.ID && p.ID == productID
                            select new { s.BYQID, s.Price };
            var data = dataQuery.FirstOrDefault();
            return GetFloatPrice(organizationID, data.BYQID, data.Price);
        }
    }
}
