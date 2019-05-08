using DistributionModel;
using ERPModelBO;
using ERPViewModelBasic;
using IWCFServiceForIM;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class BaseBillRetailVM : DistributionBillVM<BillRetail, BillRetailDetails, ProductForRetail>
    {
        private List<RetailShift> _shifts = VMGlobal.DistributionQuery.LinqOP.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.IsEnabled).ToList();
        public virtual List<RetailShift> Shifts { get { return _shifts; } }

        public VIPBO VipBO = null;

        private VIPUpgradeInfo _vipUpgradeInfo = null;

        private string _vipMessage = "";
        public string VIPMessage
        {
            get { return _vipMessage; }
            set
            {
                if (_vipMessage != value)
                {
                    _vipMessage = value;
                    OnPropertyChanged("VIPMessage");
                }
            }
        }

        private bool _isBirthdayConsume = false;

        IEnumerable<int> _couponBrandIDs = null;
        /// <summary>
        /// 抵价券应用品牌
        /// </summary>
        public IEnumerable<int> CouponBrandIDs { get { return _couponBrandIDs; } set { _couponBrandIDs = value; } }

        int _beforeDiscountCoupon = 0;
        public int BeforeDiscountCoupon { get { return _beforeDiscountCoupon; } set { _beforeDiscountCoupon = value; } }

        int _afterDiscountCoupon = 0;
        public int AfterDiscountCoupon { get { return _afterDiscountCoupon; } set { _afterDiscountCoupon = value; } }

        public BaseBillRetailVM()
        {
            this.GridDataItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GridDataItems_CollectionChanged);
        }

        void GridDataItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    ProductForRetail p = (ProductForRetail)item;
                    this.HandleGridDataItem(p);
                }
        }

        protected virtual void HandleGridDataItem(ProductForRetail item, bool isSetBirthdayDiscount = true)
        {
            decimal discount = 100.0M;
            if (this.VipBO != null && this.VipBO.Kinds != null)
            {
                var bd = this.VipBO.Kinds.Find(o => o.BrandID == item.BrandID);
                if (bd != null)
                {
                    discount = bd.Discount;
                    item.IsApplyVIPDiscount = true;
                }
            }
            item.Discount = discount;
        }

        public virtual OPResult ValidateWhenCash()
        {
            if (Master.StorageID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择出货仓库" };
            }
            int quantity = GridDataItems.Count(o => o.Quantity != 0);
            if (quantity == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            return new OPResult { IsSucceed = true };
        }

        public virtual void SetRetailData()
        {
            BillRetail retail = this.Master;
            //数量和金额置0
            retail.Quantity = 0;
            retail.CostMoney = 0.0M;
            retail.PredepositPay = 0;

            decimal totalPriceForCoupon = 0.0M;//抵价券应用的品牌原总金额(未打折时金额累加)            
            Action<DistributionProductShow> action = p =>
            {
                retail.Quantity += p.Quantity;
                retail.CostMoney += (p.Quantity * p.Price * p.Discount / 100.0M);
                if (_couponBrandIDs != null && _couponBrandIDs.Contains(p.BrandID))
                    totalPriceForCoupon += (p.Quantity * p.Price);
            };
            this.TraverseGridDataItems(action);
            //retail.CostMoney = Math.Ceiling(retail.CostMoney);//向上取整
            retail.ReceiveTicket = _beforeDiscountCoupon + _afterDiscountCoupon;

            if (retail.CostMoney > 0)//抵用券只适用于正金额
            {
                decimal beforeTicketMoney = 0.0M;//折前券抵用金额,以原金额占比计算
                if (_beforeDiscountCoupon != 0 && totalPriceForCoupon != 0)
                {
                    action = p =>
                    {
                        if (_couponBrandIDs != null && _couponBrandIDs.Contains(p.BrandID))
                            beforeTicketMoney += (p.Quantity * p.Price * p.Discount / 100.0M) * _beforeDiscountCoupon / totalPriceForCoupon;
                    };
                    this.TraverseGridDataItems(action);
                }
                retail.TicketMoney = Math.Floor(beforeTicketMoney + Math.Min(retail.CostMoney - beforeTicketMoney, _afterDiscountCoupon));//向下取整
                retail.TicketMoney = Math.Min(retail.TicketMoney, retail.CostMoney);
                if (_beforeDiscountCoupon != 0 && _afterDiscountCoupon != 0)
                    retail.TicketKind = 3;
                else if (_beforeDiscountCoupon != 0)
                    retail.TicketKind = 1;
                else if (_afterDiscountCoupon != 0)
                    retail.TicketKind = 2;
            }
            if (this.VipBO != null)
                retail.VIPID = this.VipBO.CardInfo.ID;
            retail.OrganizationID = VMGlobal.CurrentUser.OrganizationID;

            Details = new List<BillRetailDetails>();
            this.TraverseGridDataItems(p =>
            {
                Details.Add(new BillRetailDetailsForPrint
                {
                    ProductID = p.ProductID,
                    Quantity = p.Quantity,
                    Discount = p.Discount,
                    Price = p.Price,
                    ProductCode = p.ProductCode,
                    CutMoney = p.CutMoney
                });
                retail.CostMoney -= p.CutMoney;
            });
            retail.CreatorID = VMGlobal.CurrentUser.ID;
            //将下句从本方法开头移到末尾
            //retail.CostMoney = Math.Sign(retail.CostMoney) * Math.Ceiling(Math.Abs(retail.CostMoney));//向上取整(负数则向下取整)
            //retail.ReceiveMoney = retail.CostMoney - retail.TicketMoney;
            //retail.ReceiveMoney = Math.Sign(retail.ReceiveMoney) * Math.Ceiling(Math.Abs(retail.ReceiveMoney));//向上取整(负数则向下取整)
        }

        protected virtual BillRetailBO GenerateRetailBO()
        {
            var bo = new BillRetailBO { Bill = this.Master, Details = this.Details };
            var brandIDs = GridDataItems.Select(o => o.BrandID).Distinct().ToList();
            bo.BillStoreOuts = new List<BillBO<BillStoreOut, BillStoreOutDetails>>();
            bo.BillStorings = new List<BillBO<BillStoring, BillStoringDetails>>();
            foreach (var bid in brandIDs)
            {
                var storeout = this.GenerateStoreOut(bid);
                if (storeout.Details.Count > 0)
                    bo.BillStoreOuts.Add(storeout);
                var storing = this.GenerateStoring(bid);
                if (storing.Details.Count > 0)
                    bo.BillStorings.Add(storing);
            }
            return bo;
        }

        public override OPResult Save()
        {
            BillRetailBO bo = this.GenerateRetailBO();

            OPResult result = null;
            try
            {
                result = BillWebApiInvoker.Instance.Invoke<OPResult<BillRetail>, BillRetailBO>(bo, "BillRetail/SaveBillRetail");
            }
            catch (Exception ex)
            {
                result = new OPResult { IsSucceed = false, Message = ex.Message };
            }

            if (result.IsSucceed)
            {
                this.Master = ((OPResult<BillRetail>)result).Result;

                var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == OrganizationListVM.CurrentOrganization.ParentID).ToArray();
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("{2}销售{0}件,单号{1},金额{3:C}.", Details.Sum(o => o.Quantity), Master.Code, OrganizationListVM.CurrentOrganization.Name, Master.CostMoney)
                }, IMReceiveAccessEnum.零售单);
            }
            return result;
        }

        /// <summary>
        /// 生成出库单
        /// </summary>
        private BillBO<BillStoreOut, BillStoreOutDetails> GenerateStoreOut(int brandID)
        {
            BillBO<BillStoreOut, BillStoreOutDetails> bo = new BillBO<BillStoreOut, BillStoreOutDetails>();
            bo.Bill = new BillStoreOut();
            bo.Bill.Remark = "零售出库";
            bo.Bill.BillType = (int)BillTypeEnum.BillRetail;
            bo.Bill.OrganizationID = this.Master.OrganizationID;//VMGlobal.CurrentUser.OrganizationID;
            bo.Bill.StorageID = this.Master.StorageID;
            bo.Bill.BrandID = brandID;
            bo.Bill.CreatorID = this.Master.CreatorID;//VMGlobal.CurrentUser.ID;

            List<BillStoreOutDetails> soDetails = new List<BillStoreOutDetails>();
            this.TraverseGridDataItems(p =>
            {
                if (p.BrandID == brandID && p.Quantity > 0)
                {
                    soDetails.Add(new BillStoreOutDetails
                    {
                        ProductID = p.ProductID,
                        Quantity = p.Quantity
                    });
                }
            });
            bo.Details = soDetails;
            return bo;
        }

        /// <summary>
        /// 生成入库单
        /// </summary>
        private BillBO<BillStoring, BillStoringDetails> GenerateStoring(int brandID)
        {
            BillBO<BillStoring, BillStoringDetails> bo = new BillBO<BillStoring, BillStoringDetails>();
            bo.Bill = new BillStoring();
            bo.Bill.Remark = "零售入库";
            bo.Bill.BillType = (int)BillTypeEnum.BillRetail;
            bo.Bill.OrganizationID = this.Master.OrganizationID;//VMGlobal.CurrentUser.OrganizationID;
            bo.Bill.StorageID = this.Master.StorageID;
            bo.Bill.BrandID = brandID;
            bo.Bill.CreatorID = this.Master.CreatorID;//VMGlobal.CurrentUser.ID;

            List<BillStoringDetails> soDetails = new List<BillStoringDetails>();
            this.TraverseGridDataItems(p =>
            {
                if (p.BrandID == brandID && p.Quantity < 0)
                {
                    soDetails.Add(new BillStoringDetails
                    {
                        ProductID = p.ProductID,
                        Quantity = (-1) * p.Quantity
                    });
                }
            });
            bo.Details = soDetails;
            return bo;
        }

        public override void Init()
        {
            VipBO = null;
            _vipUpgradeInfo = null;
            VIPMessage = "";
            _couponBrandIDs = null;
            _beforeDiscountCoupon = 0;
            _afterDiscountCoupon = 0;
            base.Init();
        }
    }
}
