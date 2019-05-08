using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using Kernel;
using SysProcessViewModel;
using DistributionModel;
using ERPViewModelBasic;

namespace Manufacturing.ViewModel
{
    public class BillProductPlanVM : ManufacturingBillVM<BillProductPlan, BillProductPlanDetails, ProductForProduceBrush>
    {
        public BillProductPlanVM()
        {
            Master = new BillProductPlanBO();
            Master.CreateTime = DateTime.Now;
        }

        protected override List<ProductForProduceBrush> GetProductForShow(string code)
        {
            var products = base.GetProductForShow(code);
            if (products != null)
            {
                products.ForEach(o => o.DeliveryDate = DateTime.Now.AddDays(20));//默认交货日期为20天后
            }
            return products;
        }

        public OPResult ValidateWhenSave()
        {
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定生产品牌" };
            }
            return new OPResult { IsSucceed = true };
        }

        public IEnumerable<ProductQuantity> CheckOrderlimited()
        {
            var pids = this.GridDataItems.Where(o => o.Quantity != 0).Select(o => o.ProductID).ToArray();
            var oids = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Select(o => o.ID);
            //获取订单总量
            var orders = VMGlobal.DistributionQuery.LinqOP.Search<BillOrder>(o => oids.Contains(o.OrganizationID) && !o.IsDeleted);
            var orderDetails = VMGlobal.DistributionQuery.LinqOP.GetDataContext<BillOrderDetails>();
            var orderData = from order in orders
                            from od in orderDetails
                            where order.ID == od.BillID && pids.Contains(od.ProductID)
                            select new { ProductID = od.ProductID, Quantity = od.Quantity - od.QuaCancel };
            var orderResult = orderData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //获取计划生产总量
            var plans = VMGlobal.ManufacturingQuery.LinqOP.GetDataContext<BillProductPlan>();
            var planDetails = VMGlobal.ManufacturingQuery.LinqOP.GetDataContext<BillProductPlanDetails>();
            var planData = from plan in plans
                           from pd in planDetails
                           where plan.ID == pd.BillID && pids.Contains(pd.ProductID)
                           select new { ProductID = pd.ProductID, Quantity = pd.Quantity - pd.QuaCancel };
            var planResult = planData.GroupBy(o => o.ProductID).Select(g => new ProductQuantity { ProductID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //比较
            return this.GridDataItems.Where(o => o.Quantity != 0).Select(o =>
            {
                var order = orderResult.Find(r => r.ProductID == o.ProductID);
                if (order == null)
                    order = new ProductQuantity();
                var plan = planResult.Find(r => r.ProductID == o.ProductID);
                if (plan == null)
                    plan = new ProductQuantity();
                return new ProductQuantity { ProductID = o.ProductID, Quantity = order.Quantity - plan.Quantity - o.Quantity };
            }).Where(o => o.Quantity < 0).ToList();
        }

        public override OPResult Save()
        {
            Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            Details = new List<BillProductPlanDetails>();
            TraverseGridDataItems(p =>
            {
                Details.Add(new BillProductPlanDetails { ProductID = p.ProductID, Quantity = p.Quantity, DeliveryDate = p.DeliveryDate });
            });
            if (Details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            return base.Save();
        }
    }
}
