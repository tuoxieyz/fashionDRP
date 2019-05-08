using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using ERPViewModelBasic;
using Kernel;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class BillSubcontractVM : ManufacturingBillVM<BillSubcontract, BillSubcontractDetails, ProductForProduceBrush>
    {
        public BillSubcontractVM()            
        {
            Master = new BillSubcontractBO();
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
            if (Master.OuterFactoryID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定生产工厂" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定生产品牌" };
            }
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            Details = new List<BillSubcontractDetails>();
            TraverseGridDataItems(p =>
            {
                Details.Add(new BillSubcontractDetails { ProductID = p.ProductID, Quantity = p.Quantity, DeliveryDate = p.DeliveryDate });
            });
            if (Details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            return base.Save();
        }
    }
}
