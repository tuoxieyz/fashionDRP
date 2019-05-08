using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using DistributionViewModel;
using SysProcessModel;
using System.Transactions;
using Kernel;
using SysProcessViewModel;
using ERPViewModelBasic;
using IWCFServiceForIM;
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillRetailVM : BaseBillRetailVM
    {
        #region 辅助类

        private class DiscountTacticProductMapping
        {
            public int TacticID { get; set; }
            public string TacticName { get; set; }
            public int ProductID { get; set; }
        }

        private class CostCutTacticProductMapping
        {
            public int TacticID { get; set; }
            public string TacticName { get; set; }
            public int CostMoney { get; set; }
            public int CutMoney { get; set; }
            public IEnumerable<int> ProductIDs { get; set; }
        }

        #endregion
       
        List<DiscountTacticProductMapping> _discountTacticProductMapping = new List<DiscountTacticProductMapping>();
        string _retailTacticRemark = "";//零售策略备注        

        protected override void HandleGridDataItem(ProductForRetail item, bool isSetBirthdayDiscount = true)
        {
            base.HandleGridDataItem(item, isSetBirthdayDiscount);

            var tactic = GetRetailTacticForProduct(item.ProductID, o => o.Kind == 2 || o.Kind == 3);            
            if (tactic != null)
            {
                if (tactic.CanVIPApply)
                {
                    item.Discount *= (tactic.Discount.Value / 100.0M);
                    item.IsApplyVIPDiscount = true;
                }
                else
                {
                    item.Discount = tactic.Discount.Value;
                    item.IsApplyVIPDiscount = false;
                }
                _discountTacticProductMapping.Add(new DiscountTacticProductMapping { TacticID = tactic.ID, TacticName = tactic.Name, ProductID = item.ProductID });
            }
        }

        #region 零售策略

        /// <summary>
        /// 取得条码对应的零售策略
        /// <remarks>若有多个零售策略，则取最近创建的一个,且优先为本机构创建的策略</remarks>
        /// </summary>
        private RetailTactic GetRetailTacticForProduct(int productID, System.Linq.Expressions.Expression<Func<RetailTactic, bool>> kindCondtion = null)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var products = VMGlobal.DistributionQuery.QueryProvider.GetTable<Product>("SysProcess.dbo.Product").Where(o => o.ID == productID);
            //取得当前有效零售策略
            var tactics = lp.Search<RetailTactic>(o => o.BeginDate <= DateTime.Now.Date && (o.EndDate == null || o.EndDate >= DateTime.Now.Date));
            if (kindCondtion != null)
                tactics = tactics.Where(kindCondtion);
            tactics = GetRetailTacticForProduct(products, tactics);
            return tactics.OrderByDescending(o => o.OrganizationID).ThenByDescending(o => o.ID).FirstOrDefault();
        }

        /// <summary>
        /// 获取相应条码的所有满减策略
        /// <remarks>若一个条码应用了多个策略，则只有最近的策略有效(就近原则)</remarks>
        /// </summary>
        private IEnumerable<CostCutTacticProductMapping> GetCostCutTacticForProduct(IEnumerable<int> productIDs)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var products = VMGlobal.DistributionQuery.QueryProvider.GetTable<Product>("SysProcess.dbo.Product").Where(o => productIDs.Contains(o.ID));
            //取得当前有效零售策略
            var tactics = lp.Search<RetailTactic>(o => o.BeginDate <= DateTime.Now.Date && (o.EndDate == null || o.EndDate >= DateTime.Now.Date) && (o.Kind == 1 || o.Kind == 3));
            tactics = tactics.OrderByDescending(o => o.OrganizationID).ThenByDescending(o => o.ID);
            //tactics = GetRetailTacticForProduct(products, tactics);
            var organizations = lp.Search<ViewOrganization>(o => o.ID == VMGlobal.CurrentUser.OrganizationID);
            var mappings = lp.GetDataContext<RetailTacticProStyleMapping>();
            var tempmappings = from mapping in mappings
                               from product in products
                               where product.StyleID == mapping.StyleID
                               select new { mapping.TacticID, ProductID = product.ID };
            var data = from tactic in tactics
                       from mapping in tempmappings
                       where mapping.TacticID == tactic.ID
                       from organization in organizations
                       where tactic.OrganizationID == organization.ID || tactic.OrganizationID == organization.ParentID//只取本机构或父级机构设置的策略
                       select new { TacticID = tactic.ID, TacticName = tactic.Name, CostMoney = tactic.CostMoney.Value, CutMoney = tactic.CutMoney.Value, ProductID = mapping.ProductID };
            var tempdata = data.ToList();
            if (tempdata.Count > 0)
            {
                List<CostCutTacticProductMapping> tpmappings = new List<CostCutTacticProductMapping>();
                var tacticIDs = tempdata.Select(o => o.TacticID).Distinct().ToList();
                foreach (var tid in tacticIDs)
                {
                    var tactic = tempdata.FirstOrDefault(o => o.TacticID == tid);
                    if (tactic != null)
                    {
                        var pids = tempdata.Where(o => o.TacticID == tid).Select(o => o.ProductID).ToList();
                        tpmappings.Add(new CostCutTacticProductMapping
                        {
                            TacticID = tid,
                            CostMoney = tactic.CostMoney,
                            CutMoney = tactic.CutMoney,
                            TacticName = tactic.TacticName,
                            ProductIDs = pids
                        });
                        tempdata.RemoveAll(o => pids.Contains(o.ProductID));//移除已找到策略的条码，防止多个策略同时应用
                    }
                }
                return tpmappings;
            }
            return null;
        }

        private IQueryable<RetailTactic> GetRetailTacticForProduct(IQueryable<Product> products, IQueryable<RetailTactic> tactics)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var organizations = lp.Search<ViewOrganization>(o => o.ID == VMGlobal.CurrentUser.OrganizationID);
            var mappings = lp.GetDataContext<RetailTacticProStyleMapping>();
            mappings = from mapping in mappings
                       from product in products
                       where product.StyleID == mapping.StyleID
                       select mapping;
            tactics = from tactic in tactics //服了服了，假如把这一行和下一行调换一下顺序竟然就运行时报错
                      from mapping in mappings
                      where mapping.TacticID == tactic.ID
                      from organization in organizations
                      where tactic.OrganizationID == organization.ID || tactic.OrganizationID == organization.ParentID//只取本机构或父级机构设置的策略
                      //orderby tactic.OrganizationID
                      select tactic;
            return tactics;
        }

        #endregion

        public override void SetRetailData()
        {
            Action<DistributionProductShow> action = p =>
            {
                this.Master.Quantity += p.Quantity;
                this.Master.CostMoney += (p.Quantity * p.Price * p.Discount / 100.0M);
            };
            this.TraverseGridDataItems(action);

            if (this.Master.Remark == _retailTacticRemark)
                this.Master.Remark = _retailTacticRemark = "";

            if (_discountTacticProductMapping.Count > 0)
            {
                _discountTacticProductMapping.RemoveAll(o => !(GridDataItems.Where(d => d.Quantity != 0).Select(d => d.ProductID).Contains(o.ProductID)));
                var dtactics = _discountTacticProductMapping.Select(o => o.TacticName).Distinct();
                _retailTacticRemark = string.Join(",", dtactics) + ",";
            }

            //零售满减策略            
            //if (retail.CostMoney > 0)
            //{
            var details = GridDataItems.Where(o => o.Quantity != 0);
            var cctactics = GetCostCutTacticForProduct(details.Select(o => o.ProductID));
            if (cctactics != null)
            {
                decimal costMoney = this.Master.CostMoney;
                foreach (var cctactic in cctactics)
                {
                    var temp = details.Where(o => cctactic.ProductIDs.Contains(o.ProductID));
                    var costprice = temp.Sum(o => o.Quantity * o.Price * o.Discount / 100.0M);
                    if (costprice >= cctactic.CostMoney)
                    {
                        int times = (int)costprice / cctactic.CostMoney;//倍数
                        var cutMoney = Math.Min(this.Master.CostMoney, cctactic.CutMoney * times);
                        costMoney -= cutMoney;
                        _retailTacticRemark += cctactic.TacticName + ",";
                        foreach (var d in temp)
                        {
                            d.CutMoney = (d.Price * d.Quantity * d.Discount * cutMoney / (100 * costprice));
                        }
                        if (costMoney == 0)
                            break;
                    }
                }
            }
            // }            
            if (string.IsNullOrWhiteSpace(this.Master.Remark) && !string.IsNullOrEmpty(_retailTacticRemark))
            {
                this.Master.Remark = _retailTacticRemark.TrimEnd(',');
            }

            base.SetRetailData();
        }

        public override void Init()
        {
            List<DiscountTacticProductMapping> _discountTacticProductMapping = new List<DiscountTacticProductMapping>();
            _retailTacticRemark = "";//零售策略备注
            base.Init();
        }
    }
}
