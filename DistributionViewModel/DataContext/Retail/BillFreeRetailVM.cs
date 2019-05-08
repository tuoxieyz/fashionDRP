using DistributionModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class BillFreeRetailVM : DistributionBillVM<BillRetail, BillRetailDetails, ProductForRetail>
    {
        #region 辅助类

        private class VIPInfoForRetail : VIPCard
        {
            public bool IsBirthday { get; set; }
            public decimal BirthdayDiscount { get; set; }
            private int _pointTimes = 1;
            public int PointTimes { get { return _pointTimes; } set { _pointTimes = value; } }
            public List<VIPKind> VIPKinds { get; set; }
            /// <summary>
            /// 应用了vip折扣的商品数量
            /// </summary>
            public int Quantity { get; set; }
            /// <summary>
            /// 应用了vip折扣的商品折前价合计
            /// </summary>
            public int CostMoney { get; set; }
        }

        #endregion

        private List<RetailShift> _shifts = VMGlobal.DistributionQuery.LinqOP.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && o.IsEnabled).ToList();
        public List<RetailShift> Shifts { get { return _shifts; } }

        private VIPInfoForRetail _vipInfo = null;

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

        IEnumerable<int> _couponBrandIDs = null;
        /// <summary>
        /// 抵价券应用品牌
        /// </summary>
        public IEnumerable<int> CouponBrandIDs { get { return _couponBrandIDs; } set { _couponBrandIDs = value; } }

        int _beforeDiscountCoupon = 0;
        public int BeforeDiscountCoupon { get { return _beforeDiscountCoupon; } set { _beforeDiscountCoupon = value; } }

        int _afterDiscountCoupon = 0;
        public int AfterDiscountCoupon { get { return _afterDiscountCoupon; } set { _afterDiscountCoupon = value; } }

        VIPBirthdayTactic _birthdayTactic = null;
        private VIPBirthdayTactic BirthdayTactic
        {
            get
            {
                if (_birthdayTactic == null)
                    _birthdayTactic = this.GetVIPBirthdayTactic();
                return _birthdayTactic;
            }
        }

        public BillFreeRetailVM()
        {
            this.GridDataItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GridDataItems_CollectionChanged);
        }

        private void HandleGridDataItem(ProductForRetail item, bool notforBirthday = false)
        {
            var tactic = GetRetailTacticForProduct(item.ProductID, o => o.Kind == 2 || o.Kind == 3);
            decimal discount = 100.0M;
            if (this._vipInfo != null && this._vipInfo.VIPKinds != null)
            {
                var bd = this._vipInfo.VIPKinds.Find(o => o.BrandID == item.BrandID);
                if (bd != null)
                {
                    if (this._vipInfo.IsBirthday && !notforBirthday)
                        discount = this._vipInfo.BirthdayDiscount;
                    else
                        discount = bd.Discount;
                    item.IsApplyVIPDiscount = true;
                }
            }
            if (tactic != null)
            {
                if (tactic.CanVIPApply)
                {
                    discount *= (tactic.Discount.Value / 100.0M);
                    item.IsApplyVIPDiscount = true;
                }
                else
                {
                    discount = tactic.Discount.Value;
                    item.IsApplyVIPDiscount = false;
                }
                _discountTacticProductMapping.Add(new DiscountTacticProductMapping { TacticID = tactic.ID, TacticName = tactic.Name, ProductID = item.ProductID });
            }
            item.Discount = discount;
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

        #region VIP处理

        /// <summary>
        /// 取得VIP生日消费策略
        /// <remarks>只取本机构或父级机构设置的策略,且优先为本机构创建的策略</remarks>
        /// </summary>
        private VIPBirthdayTactic GetVIPBirthdayTactic()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var organizations = lp.Search<ViewOrganization>(o => o.ID == VMGlobal.CurrentUser.OrganizationID);
            var tactics = lp.GetDataContext<VIPBirthdayTactic>();
            tactics = from tactic in tactics
                      from organization in organizations
                      where tactic.OrganizationID == organization.ID || tactic.OrganizationID == organization.ParentID
                      select tactic;
            return tactics.OrderByDescending(o => o.OrganizationID).FirstOrDefault();
        }

        /// <summary>
        /// 根据VIPID获取对应的类型集合
        /// </summary>
        private List<VIPKind> GetVIPKinds(int vid)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var cks = lp.Search<VIPCardKindMapping>(o => o.CardID == vid);
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var vipkinds = lp.Search<VIPKind>(o => brandIDs.Contains(o.BrandID));
            var data = from vk in vipkinds
                       from ck in cks
                       where vk.ID == ck.KindID
                       select vk;
            return data.ToList();
        }

        /// <summary>
        /// 获取VIP总积分
        /// </summary>
        private int GetVIPPoint(int vid)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            return lp.Search<VIPPointTrack>(o => o.VIPID == vid).Sum(o => o.Point);
        }

        public void CleanVIP()
        {
            this._vipInfo = null;
            this.VIPMessage = "";
            this.TraverseGridDataItems(p => this.HandleGridDataItem(p));
        }

        public void SetVIP(VIPCard vip)
        {
            this._vipInfo = new VIPInfoForRetail
            {
                ID = vip.ID,
                Birthday = vip.Birthday,
                Code = vip.Code,
                MobilePhone = vip.MobilePhone,
                CustomerName = vip.CustomerName,
                Sex = vip.Sex
            };
            string info = vip.CustomerName + (vip.Sex ? "先生" : "女士") + "您好,";
            this._vipInfo.VIPKinds = this.GetVIPKinds(vip.ID);
            if (this._vipInfo.VIPKinds != null && this._vipInfo.VIPKinds.Count > 0)
            {
                if (DateTime.Now.Month == vip.Birthday.Month && DateTime.Now.Day == vip.Birthday.Day)//当天生日
                {
                    this._vipInfo.IsBirthday = true;
                    info += "生日快乐！";
                    var birthdayTactic = this.BirthdayTactic;
                    if (birthdayTactic != null)
                    {
                        info += "今日您能享受";
                        foreach (var kind in this._vipInfo.VIPKinds)
                        {
                            info += (VMGlobal.PoweredBrands.Find(o => o.ID == kind.BrandID).Name + ",");
                        }
                        info = info.TrimEnd(',');
                        if (birthdayTactic.QuantityLimit != null && birthdayTactic.QuantityLimit != 0)
                            info += string.Format("{0}件以内、", birthdayTactic.QuantityLimit);
                        if (birthdayTactic.MoneyLimit != null && birthdayTactic.MoneyLimit != 0)
                            info += string.Format("折前价合计{0}元", birthdayTactic.MoneyLimit);
                        info = info.TrimEnd('、');
                        info += "商品";
                        if (birthdayTactic.PointTimes != 1)
                        {
                            this._vipInfo.PointTimes = birthdayTactic.PointTimes;
                            info += string.Format("{0}倍积分,", birthdayTactic.PointTimes);
                        }
                        this._vipInfo.BirthdayDiscount = birthdayTactic.Discount;
                        info += string.Format("{0}折扣,", birthdayTactic.Discount);
                    }
                }
                else
                {
                    info += "您能享受";
                    foreach (var kind in this._vipInfo.VIPKinds)
                    {
                        info += (VMGlobal.PoweredBrands.Find(o => o.ID == kind.BrandID).Name + kind.Name + kind.Discount.ToString() + "折扣,");
                    }
                }
            }
            info += string.Format("您的当前积分为{0}", this.GetVIPPoint(vip.ID));
            this.TraverseGridDataItems(p => this.HandleGridDataItem(p));
            VIPMessage = info;
        }

        private IEnumerable<VIPUpTactic> GetVIPUpTacticsWhenCash()
        {
            if (_vipInfo == null)
                return null;
            else
            {
                List<VIPUpTactic> reTactics = new List<VIPUpTactic>();
                var lp = VMGlobal.DistributionQuery.LinqOP;
                var brandIDs = GridDataItems.Select(o => o.BrandID).Distinct();
                var kindIDs = _vipInfo.VIPKinds.Where(o => brandIDs.Contains(o.BrandID)).Select(o => o.ID);
                var tactics = lp.Search<VIPUpTactic>(o => kindIDs.Contains(o.FormerKindID) && o.IsEnabled).ToList();
                foreach (var tactic in tactics)
                {
                    if (tactic.OnceConsume != 0)
                    {
                        var pids = GridDataItems.Where(o => o.BrandID == tactic.BrandID).Select(o => o.ProductID);
                        var costMoney = Details.Where(o => pids.Contains(o.ProductID)).Sum(o => o.Quantity * o.Price * o.Discount / 100.0M - o.CutMoney);
                        if (costMoney >= tactic.OnceConsume)
                        {
                            reTactics.Add(tactic);
                            continue;
                        }
                    }
                    if (tactic.DateSpan != 0 && tactic.SpanConsume != 0)
                    {
                        var beginDate = DateTime.Now.AddDays(tactic.DateSpan * (-1)).Date;
                        var retails = lp.Search<BillRetail>(o => o.CreateTime >= beginDate && o.VIPID == _vipInfo.ID);
                        var details = lp.GetDataContext<BillRetailDetails>();
                        var products = lp.Search<ViewProduct>(o => o.BrandID == tactic.BrandID);
                        var data = from detail in details
                                   from retail in retails
                                   where retail.ID == detail.BillID
                                   from product in products
                                   where detail.ProductID == product.ProductID
                                   select detail;
                        var totalCost = data.Sum(o => o.Quantity * o.Price * o.Discount / 100.0M - o.CutMoney);
                        if (totalCost >= tactic.SpanConsume)
                        {
                            reTactics.Add(tactic);
                            continue;
                        }
                    }
                }
                return reTactics.OrderBy(o => o.FormerKindID);
            }
        }

        /// <summary>
        /// 根据消费情况获取VIP的升级详情
        /// </summary>
        public VIPUpgradeInfo GetVIPUpgradeInfo()
        {
            var tactics = this.GetVIPUpTacticsWhenCash();
            if (tactics != null && tactics.Count() > 0)
            {
                var kinds = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>().ToList();
                List<VIPUpTacticForCheck> cktactics = new List<VIPUpTacticForCheck>();
                foreach (var t in tactics)
                {
                    cktactics.Add(new VIPUpTacticForCheck
                    {
                        BrandID = t.BrandID,
                        FormerKindID = t.FormerKindID,
                        AfterKindID = t.AfterKindID,
                        CutPoint = t.CutPoint,
                        FormerKindName = kinds.Find(o => o.ID == t.FormerKindID).Name,
                        AfterKindName = kinds.Find(o => o.ID == t.AfterKindID).Name,
                        BrandName = VMGlobal.PoweredBrands.Find(o => o.ID == t.BrandID).Name
                    });
                }
                _vipUpgradeInfo = new VIPUpgradeInfo { VIPInfo = _vipInfo.CustomerName + (_vipInfo.Sex ? "先生" : "女士") + ",您的VIP卡可以升级啦!", UpTactics = cktactics };
                return _vipUpgradeInfo;
            }
            return null;
        }

        #endregion

        public OPResult ValidateWhenCash()
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

        public OPResult ValidateVIPBirthdayConsumption()
        {
            if (this._vipInfo != null && this._vipInfo.IsBirthday && BirthdayTactic != null)
            {
                bool qualimit = false, moneylimit = false;
                string errormsg = "";
                var vbc = VMGlobal.DistributionQuery.LinqOP.Search<VIPBirthdayConsumption>(o => o.VIPID == _vipInfo.ID && o.ConsumeDay == DateTime.Now.Date).FirstOrDefault();
                if (BirthdayTactic.QuantityLimit != null && BirthdayTactic.QuantityLimit != 0)
                {
                    int vipQuantity = (vbc == null ? 0 : vbc.Quantity);
                    this.TraverseGridDataItems(p =>
                    {
                        if (p.IsApplyVIPDiscount)
                            vipQuantity += p.Quantity;
                    });
                    this._vipInfo.Quantity = vipQuantity;
                    qualimit = BirthdayTactic.QuantityLimit < vipQuantity;//数量超出
                    errormsg += string.Format("当日享受生日折扣的商品最多购买{0}件，现已超出{1}件", BirthdayTactic.QuantityLimit, vipQuantity - BirthdayTactic.QuantityLimit);
                }
                if (BirthdayTactic.MoneyLimit != null && BirthdayTactic.MoneyLimit != 0)
                {
                    decimal vipMoney = (vbc == null ? 0 : vbc.CostMoney);
                    this.TraverseGridDataItems(p =>
                    {
                        if (p.IsApplyVIPDiscount)
                            vipMoney += p.Price * p.Quantity;
                    });
                    this._vipInfo.CostMoney = (int)vipMoney;
                    moneylimit = BirthdayTactic.MoneyLimit < vipMoney;//金额超出
                    if (!string.IsNullOrEmpty(errormsg))
                        errormsg += "或";
                    errormsg += string.Format("当日享受生日折扣的商品折前价合计不能超过{0}元", BirthdayTactic.MoneyLimit);
                }
                if (qualimit && moneylimit)
                {
                    this._vipInfo.CostMoney = this._vipInfo.Quantity = 0;
                    return new OPResult { IsSucceed = false, Message = errormsg };
                }
            }
            return new OPResult { IsSucceed = true };
        }

        public void ClearVIPBirthdayDiscount()
        {
            this.TraverseGridDataItems(p => this.HandleGridDataItem(p, true));
        }

        public void SetRetailData()
        {
            BillRetail retail = this.Master;
            //数量和金额置0
            retail.Quantity = 0;
            retail.CostMoney = 0.0M;
            if (retail.Remark == _retailTacticRemark)
                retail.Remark = _retailTacticRemark = "";

            decimal totalPriceForCoupon = 0.0M;//抵价券应用的品牌原总金额(未打折时金额累加)            
            Action<DistributionProductForBrush> action = p =>
            {
                retail.Quantity += p.Quantity;
                retail.CostMoney += (p.Quantity * p.Price * p.Discount / 100.0M);
                if (_couponBrandIDs != null && _couponBrandIDs.Contains(p.BrandID))
                    totalPriceForCoupon += (p.Quantity * p.Price);
            };
            this.TraverseGridDataItems(action);
            retail.CostMoney = Math.Ceiling(retail.CostMoney);//向上取整
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
            if (this._vipInfo != null)
                retail.VIPID = this._vipInfo.ID;
            retail.OrganizationID = VMGlobal.CurrentUser.OrganizationID;

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
                decimal costMoney = retail.CostMoney;
                foreach (var cctactic in cctactics)
                {
                    var temp = details.Where(o => cctactic.ProductIDs.Contains(o.ProductID));
                    var costprice = temp.Sum(o => o.Quantity * o.Price * o.Discount / 100.0M);
                    if (costprice >= cctactic.CostMoney)
                    {
                        int times = (int)costprice / cctactic.CostMoney;//倍数
                        var cutMoney = Math.Min(retail.CostMoney, cctactic.CutMoney * times);
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
            if (string.IsNullOrWhiteSpace(retail.Remark) && !string.IsNullOrEmpty(_retailTacticRemark))
            {
                retail.Remark = _retailTacticRemark.TrimEnd(',');
            }

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
            retail.ReceiveMoney = retail.CostMoney - retail.TicketMoney;
        }

        public override OPResult Save()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;

            var brandIDs = GridDataItems.Select(o => o.BrandID).Distinct().ToList();
            List<BillStoreOutVM> storeouts = new List<BillStoreOutVM>();
            List<BillStoringVM> storings = new List<BillStoringVM>();
            foreach (var bid in brandIDs)
            {
                var storeout = this.GenerateStoreOut(bid);
                if (storeout.Details.Count > 0)
                    storeouts.Add(storeout);
                var storing = this.GenerateStoring(bid);
                if (storing.Details.Count > 0)
                    storings.Add(storing);
            }
            Action storeAction = () =>
            {
                foreach (var storeout in storeouts)
                {
                    storeout.Master.RefrenceBillCode = Master.Code;
                    storeout.SaveWithNoTran();
                }
                foreach (var storing in storings)
                {
                    storing.Master.RefrenceBillCode = Master.Code;
                    storing.SaveWithNoTran();
                }
            };
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    base.SaveWithNoTran();
                    storeAction();
                    if (this._vipInfo != null)
                    {
                        //添加VIP积分记录
                        VIPPointTrack pointTrack = new VIPPointTrack
                        {
                            CreateTime = DateTime.Now,
                            Point = _vipInfo.PointTimes * ((int)Master.CostMoney),
                            VIPID = _vipInfo.ID,
                            Remark = "零售单产生,小票号" + Master.Code
                        };
                        lp.Add<VIPPointTrack>(pointTrack);
                        if (_vipUpgradeInfo != null)
                        {
                            var tactics = _vipUpgradeInfo.UpTactics.Where(o => o.IsChecked);
                            if (tactics != null && tactics.Count() > 0)
                            {
                                IEnumerable<int> kindIDs = tactics.Select(o => o.FormerKindID).ToList();
                                IEnumerable<VIPCardKindMapping> mappings = lp.Search<VIPCardKindMapping>(o => o.CardID == _vipInfo.ID && kindIDs.Contains(o.KindID)).ToList();
                                foreach (var mapping in mappings)
                                {
                                    var t = tactics.First(o => o.FormerKindID == mapping.KindID);
                                    mapping.KindID = t.AfterKindID;
                                    if (t.CutPoint != 0)
                                    {
                                        pointTrack = new VIPPointTrack
                                        {
                                            CreateTime = DateTime.Now,
                                            Point = (-1 * t.CutPoint),
                                            VIPID = _vipInfo.ID,
                                            Remark = "VIP升级产生," + t.Description
                                        };
                                        lp.Add<VIPPointTrack>(pointTrack);
                                    }
                                }
                                lp.Update<VIPCardKindMapping>(mappings);
                            }
                        }
                        if (_vipInfo.IsBirthday && this._vipInfo.CostMoney != 0 && this._vipInfo.Quantity != 0)
                        {
                            lp.Delete<VIPBirthdayConsumption>(o => o.VIPID == _vipInfo.ID && o.ConsumeDay == DateTime.Now.Date);
                            lp.Add<VIPBirthdayConsumption>(new VIPBirthdayConsumption
                            {
                                VIPID = _vipInfo.ID,
                                Quantity = _vipInfo.Quantity,
                                CostMoney = _vipInfo.CostMoney,
                                ConsumeDay = DateTime.Now.Date
                            });
                        }
                    }
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == OrganizationListVM.CurrentOrganization.ParentID).ToArray();
            IMHelper.AsyncSendMessageTo(users, new IMessage
            {
                Message = string.Format("{2}销售{0}件,单号{1},金额{3:C}.", Details.Sum(o => o.Quantity), Master.Code, OrganizationListVM.CurrentOrganization.Name, Master.CostMoney)
            }, IMReceiveAccessEnum.零售单);
            return new OPResult { IsSucceed = true };
        }

        /// <summary>
        /// 生成出库单
        /// </summary>
        private BillStoreOutVM GenerateStoreOut(int brandID)
        {
            var bill = this.Master;
            BillStoreOutVM storeout = new BillStoreOutVM();
            var soMaster = storeout.Master;
            soMaster.Remark = "零售出库";
            soMaster.BillType = (int)BillTypeEnum.BillRetail;
            soMaster.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            soMaster.StorageID = bill.StorageID;
            soMaster.BrandID = brandID;

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
            storeout.Details = soDetails;
            return storeout;
        }

        /// <summary>
        /// 生成入库单
        /// </summary>
        private BillStoringVM GenerateStoring(int brandID)
        {
            var bill = this.Master;
            BillStoringVM storing = new BillStoringVM();
            var soMaster = storing.Master;
            soMaster.Remark = "零售入库";
            soMaster.BillType = (int)BillTypeEnum.BillRetail;
            soMaster.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            soMaster.StorageID = bill.StorageID;
            soMaster.BrandID = brandID;

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
            storing.Details = soDetails;
            return storing;
        }

        public override void Init()
        {
            _vipInfo = null;
            _vipUpgradeInfo = null;
            VIPMessage = "";
            _couponBrandIDs = null;
            _beforeDiscountCoupon = 0;
            _afterDiscountCoupon = 0;
            List<DiscountTacticProductMapping> _discountTacticProductMapping = new List<DiscountTacticProductMapping>();
            _retailTacticRemark = "";//零售策略备注
            base.Init();
        }
    }
}
