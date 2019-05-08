using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ServiceModel;
using IWCFService;
using SysProcessModel;
using Kernel;
using System.Transactions;
using DomainLogicEncap;
using SysProcessViewModel;
using ERPViewModelBasic;
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillDeliveryPackageVM : DistributionBillVM<BillDelivery, BillDeliveryDetails, ProductForDelivery>
    {
        private ContractDiscountHelper _helper = new ContractDiscountHelper();
        private List<OrganizationPriceFloat> _priceFloatCache = new List<OrganizationPriceFloat>();

        public BillDeliveryPackageVM()
            : base()
        {
            this.Init();
        }

        public override void Init()
        {
            base.Init();
            if (StorageInfoVM.Storages.Count == 1)
                Master.StorageID = StorageInfoVM.Storages[0].ID;
            if (VMGlobal.PoweredBrands.Count == 1)
                Master.BrandID = VMGlobal.PoweredBrands[0].ID;
        }

        protected override List<ProductForDelivery> GetProductForShow(string code)
        {
            var ps = base.GetProductForShow(code);
            if (ps != null)
            {
                ps.ForEach(o => {
                    o.Discount = _helper.GetDiscount(o.BYQID, Master.ToOrganizationID);
                    o.FloatPrice = this.GetToOrganizationFloatPrice(Master.ToOrganizationID, o.BYQID, o.Price);
                });
            }
            return ps;
        }

        /// <summary>
        /// [根据发货价]获取下一机构的上浮价
        /// </summary>
        /// <param name="toOrganizationID">收货机构</param>
        /// <param name="price">发货价</param>
        /// <returns>上浮价</returns>
        private decimal GetToOrganizationFloatPrice(int toOrganizationID, int byqID, decimal price)
        {
            var pf = _priceFloatCache.FirstOrDefault(o => o.OrganizationID == toOrganizationID && o.BYQID == byqID);
            if (pf == null)
            {
                var temp = VMGlobal.SysProcessQuery.LinqOP.Search<OrganizationPriceFloat>(o => o.OrganizationID == toOrganizationID && o.BYQID == byqID).ToList();
                if (temp != null && temp.Count > 0)
                    pf = temp[0];
                else
                    pf = new OrganizationPriceFloat { BYQID = byqID, OrganizationID = toOrganizationID, FloatRate = 0, LastNumber = -1 };
                _priceFloatCache.Add(pf);
            }
            if (pf.LastNumber != -1)
            {
                price += pf.FloatRate * price * 0.01M;//上浮
                price *= 0.1M;
                price = decimal.Truncate(price) * 10 + pf.LastNumber;//尾数
            }
            return price;
        }

        protected override string GenerateBillCode()
        {
            DateTime time = DateTime.Now;
            using (ChannelFactory<IBillService> channelFactory = new ChannelFactory<IBillService>("BillSVC"))
            {
                IBillService service = channelFactory.CreateChannel();
                time = service.GetDateTimeOfServer();
            }
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var maxCode = lp.Search<BillDelivery>(o => o.ToOrganizationID == Master.ToOrganizationID).Where(t => t.CreateTime >= time.Date && t.CreateTime <= time.AddDays(1).Date).Max(t => t.Code);
            if (string.IsNullOrEmpty(maxCode))
            {
                int tag = (int)Enum.Parse(typeof(BillTypeEnum), typeof(BillDelivery).Name);
                string prefixion = Enum.GetName(typeof(BillCodePrefixion), tag);
                var ocode = lp.Search<ViewOrganization>(b => b.ID == Master.ToOrganizationID).Select(o => o.Code).First();
                maxCode = prefixion + ocode + "-" + time.ToString("yyyyMMdd") + "000";
            }
            int preLength = maxCode.Length - 3;
            return maxCode.Substring(0, preLength) + (Convert.ToInt32(maxCode.Substring(preLength)) + 1).ToString("000");
        }

        //protected override string GenerateBillCode()
        //{
        //    using (ChannelFactory<IBillService> channelFactory = new ChannelFactory<IBillService>("BillSVC"))
        //    {
        //        IBillService service = channelFactory.CreateChannel();
        //        return service.GenerateBillCode(typeof(BillDelivery).AssemblyQualifiedName, Master.ToOrganizationID);
        //    }
        //}

        //public override List<ProductForDelivery> GetProductForBill(string code)
        //{
        //    var ps = base.GetProductForBill(code);

        //    if (ps.Count > 0)
        //    {
        //        return ps.Select(o => new ProductForDelivery
        //        {
        //            ProductID = o.ProductID,
        //            ProductCode = o.ProductCode,
        //            BrandCode = VMGlobal.PoweredBrands.Find(b => b.ID == o.BrandID).Code,
        //            StyleCode = o.StyleCode,
        //            ColorCode = o.ColorCode,
        //            SizeCode = o.SizeCode,
        //            SizeName = o.SizeName,
        //            Price = FPHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.BYQID, o.Price),
        //            BrandID = o.BrandID,
        //            BYQID = o.BYQID
        //        }).ToList();
        //    }
        //    return null;
        //}

        public OPResult ValidateWhenSave()
        {
            if (Master.StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定发货仓库" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定发货品牌" };
            }
            if (Master.ToOrganizationID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "未指定收货机构" };
            }
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            if (string.IsNullOrEmpty(Master.Remark))
            {
                var toOrganizationName = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Find(o => o.ID == Master.ToOrganizationID).Name;
                Master.Remark = "发往" + toOrganizationName;
            }
            Master.Status = (int)BillDeliveryStatusEnum.已装箱未配送;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    base.SaveWithNoTran();
                    if (Master.IsWriteDownOrder)
                    {
                        //冲减订单
                        Details.ForEach(d => BillLogic.UpdateOrderWhenDelivery(Master.ToOrganizationID, d.ProductID, d.Quantity));
                    }
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }
    }
}
