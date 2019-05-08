using DistributionModel;
using DistributionModel.Finance;
using DomainLogicEncap;
using ERPModelBO;
using ERPViewModelBasic;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Transactions;
using Telerik.Windows.Controls;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class NoOrderAllocateForSingleOrganizationVM : CommonViewModel<AllocateEntity>
    {
        private ContractDiscountHelper _discountHelper = new ContractDiscountHelper();
        private FloatPriceHelper _fpHelper = new FloatPriceHelper();

        private int _organizationID;
        public int OrganizationID
        {
            get { return _organizationID; }
            set
            {
                _organizationID = value;
                if (_organizationID == default(int))
                    MoneyInfo = "";
                else
                {
                    MoneyInfo = GetMoneyInfo();
                }
            }
        }

        private string _moneyInfo;
        public string MoneyInfo
        {
            get { return _moneyInfo; }
            set
            {
                _moneyInfo = value;
                OnPropertyChanged("MoneyInfo");
            }
        }

        public int StorageID { get; set; }

        public string Remark { get; set; }

        private IEnumerable<ProStyle> _styles;
        public IEnumerable<ProStyle> Styles
        {
            get { return _styles; }
            set
            {
                _styles = value;
            }
        }

        private string GetMoneyInfo()
        {
            var obs = VMGlobal.SysProcessQuery.LinqOP.Search<OrganizationBrand>(ob => ob.OrganizationID == OrganizationID);
            var brands = VMGlobal.PoweredBrands.Where(o => obs.Select(b => b.BrandID).Contains(o.ID)).ToList();
            var bids = brands.Select(o => o.ID).ToArray();
            var balanceQuery = VMGlobal.DistributionQuery.LinqOP.Search<OrganizationFundAccount>(o => o.OrganizationID == OrganizationID && bids.Contains(o.BrandID));
            var balances = balanceQuery.GroupBy(o => o.BrandID).Select(g => new { BrandID = g.Key, Balance = g.Sum(o => o.AlreadyIn - o.NeedIn) }).ToList();
            var frozenMoenys = VMGlobal.DistributionQuery.LinqOP.Search<VoucherReceiveMoney>(o => o.OrganizationID == OrganizationID && bids.Contains(o.BrandID) && o.IsMoneyFrozen && o.Status)
                .GroupBy(o => o.BrandID).Select(g => new { BrandID = g.Key, ReceiveMoney = g.Sum(o => o.ReceiveMoney) }).ToList();

            var now = DateTime.Now.Date;
            var credits = VMGlobal.DistributionQuery.LinqOP.Search<OrganizationCredit>(o => o.OrganizationID == OrganizationID && bids.Contains(o.BrandID) && o.EndDate >= now).ToList();

            string info = "";
            foreach (var brand in brands)
            {
                var balance = balances.Find(o => o.BrandID == brand.ID);
                var frozenMoeny = frozenMoenys.Find(o => o.BrandID == brand.ID);
                var credit = credits.Find(o => o.BrandID == brand.ID);
                info += string.Format("{0}:{1:C}; ", brand.Name, (balance == null ? 0 : balance.Balance) - (frozenMoeny == null ? 0 : frozenMoeny.ReceiveMoney) + (credit == null ? 0 : credit.CreditMoney));
            }

            return info;
        }

        public OPResult CheckCondition()
        {
            if (StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择配货仓库." };
            }
            if (OrganizationID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择配货机构." };
            }
            if (Styles == null || Styles.Count() == 0)
            {
                return new OPResult { IsSucceed = false, Message = "请选择配货款式." };
            }
            return new OPResult { IsSucceed = true };
        }

        protected override IEnumerable<AllocateEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var stocks = ReportDataContext.GetAvailableStock(StorageID, Styles).Where(o => o.Quantity > 0);
            var pids = stocks.Select(o => o.ProductID);
            var products = lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList().OrderBy(o => o.ProductCode);

            var orders = this.GetOrderAggregation(pids);
            var result = products.Select(o =>
                {
                    var entity = new AllocateEntity
                        {
                            ProductID = o.ProductID,
                            ProductCode = o.ProductCode,
                            StyleCode = o.StyleCode,
                            SizeID = o.SizeID
                        };
                    entity.BrandCode = VMGlobal.PoweredBrands.FirstOrEmpty(b => b.ID == o.BrandID).Code;
                    entity.ColorCode = VMGlobal.Colors.FirstOrEmpty(b => b.ID == o.ColorID).Code;
                    entity.SizeName = VMGlobal.Sizes.FirstOrEmpty(b => b.ID == entity.SizeID).Name;
                    entity.AvailableQuantity = stocks.FirstOrEmpty(b => b.ProductID == o.ProductID).Quantity;
                    entity.OrderQuantity = orders.FirstOrEmpty(b => b.ProductID == o.ProductID).Quantity;
                    entity.AllocateQuantity = Math.Min(entity.AvailableQuantity, entity.OrderQuantity);
                    entity.Discount = _discountHelper.GetDiscount(o.BYQID, OrganizationID);
                    entity.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.BYQID, o.Price);
                    return entity;
                }).ToList();
            return result;
        }

        private IEnumerable<ProductQuantity> GetOrderAggregation(IEnumerable<int> pids)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var orderContext = lp.Search<BillOrder>(o => o.OrganizationID == OrganizationID);
            var orderDetailsContext = lp.GetDataContext<BillOrderDetails>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            orderContext = orderContext.Where(o => brandIDs.Contains(o.BrandID));
            var data = from order in orderContext
                       from details in orderDetailsContext
                       where order.ID == details.BillID && pids.Contains(details.ProductID) && order.IsDeleted == false
                       select new ProductQuantity
                       {
                           ProductID = details.ProductID,
                           Quantity = details.Quantity - details.QuaCancel - details.QuaDelivered
                       };
            var temp = data.GroupBy(o => o.ProductID).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).Where(g => g.Quantity != 0).ToList();
            var result = temp.Select(o => new ProductQuantity
            {
                ProductID = o.Key,
                Quantity = o.Quantity
            }).ToList();
            return result;
        }

        public OPResult Save()
        {
            if (this.Entities == null || this.Entities.Count() == 0)
                return new OPResult { IsSucceed = false, Message = "没有可保存的数据" };
            List<BillAllocateDetails> details = new List<BillAllocateDetails>();
            foreach (var o in this.Entities)
            {
                if (o.AllocateQuantity > 0)
                    details.Add(new BillAllocateDetails { ProductID = o.ProductID, Quantity = o.AllocateQuantity });
            }
            if (details.Count == 0)
                return new OPResult { IsSucceed = false, Message = "没有可保存的数据" };
            BillAllocate bill = new BillAllocate
            {
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                OrganizationID = OrganizationID,
                Status = false,
                StorageID = StorageID,
                Remark = Remark
            };
            bill.Quantity = details.Sum(o => o.Quantity);

            var result = BillWebApiInvoker.Instance.SaveBill<BillAllocate, BillAllocateDetails>(new BillBO<BillAllocate, BillAllocateDetails>()
            {
                Bill = bill,
                Details = details
            });
            if (result.IsSucceed)
                this.Entities = null;
            return result;
        }
    }

    public class AllocateEntity : DistributionProductShow
    {
        public int AvailableQuantity { get; set; }
        public int OrderQuantity { get; set; }

        public int AllocateQuantity
        {
            get { return Quantity; }
            set
            {
                Quantity = value;
                OnPropertyChanged("AllocateQuantity");
            }
        }
    }
}
