using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Data;
using DomainLogicEncap;
using SysProcessModel;
using System.Collections;
using DistributionViewModel;
using System.Transactions;
using Kernel;
using SysProcessViewModel;
using ERPViewModelBasic;
using IWCFServiceForIM;
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillDeliveryVM : BillDeliveryPackageVM
    {
        private bool _isSelfShop = false;//是否自营店

        protected virtual decimal GetTotalMoney()
        {
            decimal totalMoney = 0.0M;
            TraverseGridDataItems(p =>
            {
                //if (p.BrandID == brandID)
                totalMoney += (p.Quantity * p.Price * p.Discount);
            });
            return totalMoney / 100;
        }

        ///// <summary>
        ///// 生成出库单
        ///// </summary>
        //private BillStoreOutVM GenerateStoreOut()
        //{
        //    BillStoreOutVM storeout = BillDataContextHelper.GenerateStoreOut<BillDelivery, BillDeliveryDetails>(Master, Details);
        //    var soMaster = storeout.Master;
        //    soMaster.StorageID = Master.StorageID;
        //    soMaster.BrandID = Master.BrandID;

        //    //IEnumerable<BillDetailBase> details = this.Details;//协变(逆变一般使用在委托中)
        //    //List<BillStoreOutDetails> soDetails = new List<BillStoreOutDetails>();
        //    //foreach (var d in details)
        //    //{
        //    //    //类型转换重载，直接用BillDeliveryDetails（不必中间先转成基类BillDetailBase）应该也行
        //    //    //可惜.net框架不支持这种父子类之间的类型转换重载
        //    //    //soDetails.Add(d);
        //    //    soDetails.Add(new BillStoreOutDetails
        //    //    {
        //    //        ProductID = d.ProductID,
        //    //        Quantity = d.Quantity
        //    //    });
        //    //}
        //    //storeout.Details = soDetails;

        //    return storeout;
        //}

        /// <summary>
        /// 检查下级机构资金和资信余额是否满足发货要求
        /// </summary>
        public OPResult CheckFundSatisfyDelivery()
        {
            var oid = this.Master.ToOrganizationID;
            //var type = VMGlobal.OrganizationTypes.Find(o => o.Name == "自营店");
            //if (type != null && type.ID == VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(oid).TypeId)
            //{
            //    return new OPResult { IsSucceed = true };
            //}
            if (_isSelfShop = OrganizationListVM.IsSelfRunShop(oid))
                return new OPResult { IsSucceed = true };

            var lp = VMGlobal.DistributionQuery.LinqOP;

            var bid = this.Master.BrandID;
            var totalMoney = GetTotalMoney();
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            if (string.IsNullOrEmpty(Master.Remark))
            {
                var toOrganizationName = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Find(o => o.ID == Master.ToOrganizationID).Name;
                Master.Remark = "发往" + toOrganizationName;
            }
            var bo = new BillDeliveryBO
            {
                Bill = this.Master,
                Details = this.Details
            };
            if (!_isSelfShop)
            {
                var bid = Master.BrandID;
                var totalMoney = GetTotalMoney();
                bo.FundAccount = new OrganizationFundAccount
                {
                    BrandID = bid,
                    OrganizationID = this.Master.ToOrganizationID,
                    NeedIn = totalMoney,
                    AlreadyIn = 0.0M,
                    CreatorID = VMGlobal.CurrentUser.ID,
                    BillKind = (int)BillTypeEnum.BillDelivery,
                    Remark = "发货单生成",
                    RefrenceBillCode = this.Master.Code
                };
            }
            var result = BillWebApiInvoker.Instance.SaveBill<BillDelivery, BillDeliveryDetails>(bo);
            if (result.IsSucceed)
            {
                var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == Master.ToOrganizationID || o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToArray();
                var toName = VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(Master.ToOrganizationID).Name;
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("发往{2}{0}件,单号{1},到货后请及时入库.", Details.Sum(o => o.Quantity), Master.Code, toName),
                    Sender = IMHelper.CurrentUser
                }, IMReceiveAccessEnum.发货单);
            }

            return result;
        }
    }

    public class BillDeliveryAsBuyoutVM : BillDeliveryVM
    {
        private decimal _discount = 100;
        public decimal Discount
        {
            get
            {
                return _discount;
            }
            set
            {
                _discount = value;
            }
        }

        public override void Init()
        {
            base.Init();
            Master.DeliveryKind = 1;
        }

        //protected override decimal GetTotalMoney()
        //{
        //    decimal totalMoney = 0.0M;
        //    TraverseGridDataItems(p =>
        //    {
        //        //if (p.BrandID == brandID)
        //        totalMoney += (p.Quantity * p.Price * Discount);
        //    });
        //    return totalMoney / 100;
        //}

        protected override List<ProductForDelivery> GetProductForShow(string code)
        {
            var ps = base.GetProductForShow(code);
            if (ps != null)
            {
                ps.ForEach(o => o.Discount = Discount);
            }
            return ps;
        }
    }

    public static partial class ReportDataContext
    {
        /// <summary>
        /// 发货单明细
        /// </summary>
        public static List<TDetail> GetBillDeliveryDetails<TDetail>(int billID) where TDetail : DistributionProductShow, new()
        {
            var detailsContext = _query.LinqOP.Search<BillDeliveryDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new TDetail
                       {
                           ProductID = product.ProductID,
                           ProductCode = product.ProductCode,
                           BYQID = product.BYQID,
                           StyleCode = product.StyleCode,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Price = details.Price,
                           Quantity = details.Quantity,
                           Discount = details.Discount,
                           NameID = product.NameID
                       };

            var result = data.ToList();
            foreach (var d in result)
            {
                var color = VMGlobal.Colors.Find(o => o.ID == d.ColorID);
                if (color != null)
                {
                    d.ColorCode = color.Code;
                    d.ColorName = color.Name;
                }
                var name = VMGlobal.ProNames.Find(o => o.ID == d.NameID);
                if (name != null)
                {
                    d.ProductName = name.Name;
                }
                d.SizeName = VMGlobal.Sizes.FirstOrEmpty(o => o.ID == d.SizeID).Name;
                var byq = VMGlobal.BYQs.FirstOrEmpty(o => o.ID == d.BYQID);
                d.BrandID = byq.BrandID;
                d.BrandCode = VMGlobal.PoweredBrands.FirstOrEmpty(o => o.ID == d.BrandID).Code;
            }
            return result;
        }
    }
}
