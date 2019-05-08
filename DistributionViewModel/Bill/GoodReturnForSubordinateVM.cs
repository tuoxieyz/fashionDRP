using DistributionModel;
using DomainLogicEncap;
using ERPModelBO;
using ERPViewModelBasic;
using IWCFServiceForIM;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Windows;

namespace DistributionViewModel
{
    public class GoodReturnForSubordinateVM : DistributionBillVM<BillGoodReturn, BillGoodReturnDetails, GoodReturnProductShow>
    {
        //protected FloatPriceHelper _fpHelper = new FloatPriceHelper();
        private ContractDiscountHelper _discountHelper = new ContractDiscountHelper();

        private IEnumerable<Storage> _subordinateStorages = null;

        public IEnumerable<Storage> SubordinateStorages
        {
            get { return _subordinateStorages; }
            set
            {
                _subordinateStorages = value;
                OnPropertyChanged("SubordinateStorages");
                OnPropertyChanged("IsShowSubordinateStorages");
            }
        }

        public Visibility IsShowSubordinateStorages
        {
            get
            {
                if (SubordinateStorages != null && SubordinateStorages.Count() > 1)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 是否次品退货
        /// </summary>
        public bool IsDefectiveReturn { get; set; }

        ///// <summary>
        ///// （为下级做退货单，退货商品直接入库）入库仓库ID
        ///// </summary>
        //public int StorageID { get; set; }

        public GoodReturnForSubordinateVM()
        {
            this.Init();
            //this.GridDataItems.CollectionChanged += GridDataItems_CollectionChanged;
        }

        public override void Init()
        {
            base.Init();

            var bill = new BillGoodReturnForSubordinate();
            bill.PropertyChanged += bill_PropertyChanged;
            this.Master = bill;
            if (VMGlobal.PoweredBrands.Count == 1)
                Master.BrandID = VMGlobal.PoweredBrands[0].ID;
        }

        void bill_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SubordinateOrganizationID")
            {
                this.SubordinateStorages = VMGlobal.DistributionQuery.LinqOP.Search<Storage>(o => o.OrganizationID == Master.OrganizationID && o.Flag).ToList();
            }
        }

        //protected void GridDataItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        ActDatasWhenBinding(e.NewItems);
        //    }
        //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset && GridDataItems.Count > 0)//微软的Reset害死人，参看 http://msdn.microsoft.com/zh-cn/library/system.collections.specialized.notifycollectionchangedaction.aspx
        //    {
        //        ActDatasWhenBinding(GridDataItems);
        //    }
        //}

        //protected virtual void ActDatasWhenBinding(IEnumerable items)
        //{
        //    foreach (var item in items)
        //    {
        //        DistributionProductForBrush p = (DistributionProductForBrush)item;
        //        p.Discount = _discountHelper.GetDiscount(p.BYQID, Master.OrganizationID);
        //        p.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, p.BrandID, p.Year, p.Quarter, p.Price);
        //    }
        //}

        protected override List<GoodReturnProductShow> GetProductForShow(string code)
        {
            var ps = base.GetProductForShow(code);
            if (ps != null)
            {
                ps.ForEach(p => p.Discount = _discountHelper.GetDiscount(p.BYQID, Master.OrganizationID));
            }
            return ps;
        }

        public virtual OPResult ValidateWhenSave()
        {
            if (Master.OrganizationID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择退货机构" };
            }
            if (Master.BrandID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请选择退货品牌" };
            }
            if (SubordinateStorages != null && SubordinateStorages.Count() != 0)
            {
                if (SubordinateStorages.Count() == 1)
                    Master.StorageID = SubordinateStorages.First().ID;
                else if (Master.StorageID == default(int))
                    return new OPResult { IsSucceed = false, Message = "请选择下级机构的退货仓库" };
            }
            //if (StorageID == default(int))
            //{
            //    return new OPResult { IsSucceed = false, Message = "请选择入库仓库" };
            //}
            return new OPResult { IsSucceed = true };
        }

        public override OPResult Save()
        {
            if (!IsDefectiveReturn && !OrganizationListVM.IsSelfRunShop(Master.OrganizationID))
            {
                var result = this.CheckGoodReturnRate(Master.OrganizationID);
                if (!result.IsSucceed)
                    return result;
            }
            this.Master.TotalPrice = this.GridDataItems.Sum(o => o.Quantity * o.Price * o.Discount) / 100;//本单退货金额
            this.Master.Quantity = this.GridDataItems.Sum(o => o.Quantity);
            this.Master.Status = (int)BillGoodReturnStatusEnum.未审核;
            //this.Master.StorageID = (-1) * StorageID;//入库仓库ID以负数形式保存到数据库，用以后续审核入库
            var details = this.Details = new List<BillGoodReturnDetails>();
            TraverseGridDataItems(p =>
            {
                details.Add(new BillGoodReturnDetails { ProductID = p.ProductID, Quantity = p.Quantity, Discount = p.Discount, Price = p.Price });
            });
            if (details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            OPResult opresult = null;
            if (Master.StorageID == default(int))
                opresult = base.Save();
            else
                opresult = BillWebApiInvoker.Instance.SaveBill<BillGoodReturn, BillGoodReturnDetails>(new BillBO<BillGoodReturn, BillGoodReturnDetails>()
                {
                    Bill = this.Master,
                    Details = this.Details
                });
            if (opresult.IsSucceed)
            {
                var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == OrganizationListVM.CurrentOrganization.ParentID).ToArray();
                IMHelper.AsyncSendMessageTo(users, new IMessage
                {
                    Message = string.Format("{2}退货{0}件,单号{1},请注意查收.", Details.Sum(o => o.Quantity), Master.Code, OrganizationListVM.CurrentOrganization.Name)
                }, IMReceiveAccessEnum.退货单);
                this.Init();
            }
            return opresult;
        }

        /// <summary>
        /// 检查退货率
        /// </summary>
        protected OPResult CheckGoodReturnRate(int organizationID)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var rate = lp.Search<OrganizationGoodReturnRate>(o => o.BrandID == this.Master.BrandID && o.OrganizationID == organizationID).FirstOrDefault();
            if (rate != null)
            {
                //获取年度季度退货率
                IEnumerable<string> yqs = this.GridDataItems.Where(o => o.Quantity != 0).Select(o => (o.Year + "-" + o.Quarter.ToString())).Distinct().ToList();
                var yqrateSource = lp.Search<OrganizationGoodReturnRatePerQuarter>(o => o.RateID == rate.ID && yqs.Contains(o.Year.ToString() + "-" + o.Quarter.ToString())).ToList();
                IEnumerable<OrganizationGoodReturnRatePerQuarter> yqrates = yqs.Select(o =>
                {
                    OrganizationGoodReturnRatePerQuarter yqrate = yqrateSource.FirstOrDefault(yqr => o == yqr.Year.ToString() + "-" + yqr.Quarter.ToString());
                    if (yqrate == null)
                    {
                        var yq = o.Split('-');
                        yqrate = new OrganizationGoodReturnRatePerQuarter { Year = Convert.ToInt32(yq[0]), Quarter = Convert.ToInt32(yq[1]), GoodReturnRate = rate.GoodReturnRate };
                    }
                    return yqrate;
                });

                IQueryable<Product> products = VMGlobal.DistributionQuery.QueryProvider.GetTable<Product>("SysProcess.dbo.Product");
                IQueryable<ProStyle> prostyles = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProStyle>("SysProcess.dbo.ProStyle");
                IQueryable<ProBYQ> byqs = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProBYQ>("SysProcess.dbo.ProBYQ");
                var deliveries = lp.GetDataContext<BillDelivery>();
                var deDetails = lp.GetDataContext<BillDeliveryDetails>();
                var goodreturns = lp.GetDataContext<BillGoodReturn>();
                var reDetails = lp.GetDataContext<BillGoodReturnDetails>();
                //var billSnapshots = lp.GetDataContext<BillSnapshot>();
                //var ssDetails = lp.GetDataContext<BillSnapshotDetailsWithUniqueCode>();
                //var mappings = lp.GetDataContext<ViewProductUniqueCodeMapping>();
                foreach (var rateitem in yqrates)
                {
                    IQueryable<Product> tempProducts = from prostyle in prostyles
                                                       from byq in byqs
                                                       where byq.ID == prostyle.BYQID && byq.BrandID == this.Master.BrandID && byq.Year == rateitem.Year && byq.Quarter == rateitem.Quarter
                                                       from product in products
                                                       where product.StyleID == prostyle.ID
                                                       select product;
                    IQueryable<BillDeliveryDetails> tempDeDetails = from delivery in deliveries
                                                                    from dd in deDetails
                                                                    where delivery.ID == dd.BillID && delivery.ToOrganizationID == organizationID && delivery.DeliveryKind == 0 && delivery.Status != (int)BillDeliveryStatusEnum.已装箱未配送 //&& delivery.BrandID == this.Master.BrandID
                                                                    from product in tempProducts
                                                                    where product.ID == dd.ProductID
                                                                    select dd;
                    var deliveriedMoney = tempDeDetails.Sum(dd => dd.Quantity * dd.Price * dd.Discount);//已发货金额    
                    if (deliveriedMoney <= 0)
                        return new OPResult { IsSucceed = false, Message = string.Format("无法退货,{0}年第{1}季货品没有发过货，无法退货", rateitem.Year, rateitem.Quarter) };

                    #region 按唯一码原发货时折扣价格作为退货时的金额计算依据，但是逻辑太复杂了，因此改为退货金额直接使用当前折扣计算（一般来说同季商品的发货折扣应该是不会变的，退货折扣等于发货折扣）
                    //var returnedUniqueCodes = (from goodreturn in goodreturns
                    //                           from rd in reDetails
                    //                           where goodreturn.ID == rd.BillID && goodreturn.OrganizationID == VMGlobal.CurrentUser.OrganizationID && goodreturn.BrandID == this.Master.BrandID
                    //                           from product in tempProducts
                    //                           where product.ID == rd.ProductID
                    //                           from snapshot in billSnapshots
                    //                           where goodreturn.Code == snapshot.BillCode
                    //                           from sd in ssDetails
                    //                           where snapshot.ID == sd.SnapshotID
                    //                           from mapping in mappings
                    //                           where sd.UniqueCode == mapping.UniqueCode && mapping.ProductID == product.ID
                    //                           select sd.UniqueCode).ToList(); //获取已退货唯一码
                    //IEnumerable<string> tempReturnedUniqueCodes = returnedUniqueCodes.Select(rc=>rc.UniqueCode).Distinct().ToArray();
                    //var recentDeliveries = (from snapshot in billSnapshots
                    //                        from sd in ssDetails
                    //                        where snapshot.ID == sd.SnapshotID && snapshot.BillTypeName == "发货单" && tempReturnedUniqueCodes.Contains(sd.UniqueCode)
                    //                        from delivery in deliveries
                    //                        where snapshot.BillCode == delivery.Code && delivery.ToOrganizationID == VMGlobal.CurrentUser.OrganizationID && delivery.DeliveryKind == 0 && delivery.Status != (int)BillDeliveryStatusEnum.已装箱未配送 && delivery.BrandID == this.Master.BrandID
                    //                        select new { snapshot.BillCode, sd.UniqueCode,snapshot.CreateTime }).ToList();
                    //List<string> exceptDeliveries = new List<string>();
                    //foreach (var rc in tempReturnedUniqueCodes)
                    //{
                    //    //当最近的一次发货时间大于最近的一次退货时间
                    //    var recentDeliveryTime = recentDeliveries.Where(o => o.UniqueCode == rc).Max(o => o.CreateTime);
                    //    var recentReturnTime = returnedUniqueCodes.Where(o => o.UniqueCode == rc).Max(o => o.CreateTime);
                    //    if (recentDeliveryTime > recentReturnTime)
                    //        exceptDeliveries.Add(recentDeliveries.Find(o => o.UniqueCode == rc && o.CreateTime == recentDeliveryTime).BillCode);
                    //}
                    //退货自动计算退货金额的逻辑并不准确，只能做到大部分情况下（同一个唯一码退货之后不会重新发给同一个机构）不出错，否则逻辑就太复杂了
                    //以后将改成在扫单时候就判断唯一码在发货时的发货金额，然后保存时只要简单汇总即可。同时新建一个表专门存放发货总金额和退货总金额
                    //uniqueCodes = uniqueCodes.Concat(returnedUniqueCodes);
                    //tempDeDetails = from snapshot in billSnapshots
                    //                from sd in ssDetails
                    //                where snapshot.ID == sd.SnapshotID && snapshot.BillTypeName == "发货单" && uniqueCodes.Contains(sd.UniqueCode)
                    //                from delivery in deliveries
                    //                where snapshot.BillCode == delivery.Code && delivery.ToOrganizationID == VMGlobal.CurrentUser.OrganizationID && delivery.DeliveryKind == 0 && delivery.Status != (int)BillDeliveryStatusEnum.已装箱未配送 && delivery.BrandID == this.Master.BrandID
                    //                from dd in deDetails
                    //                where delivery.ID == dd.BillID
                    //                from product in tempProducts
                    //                where product.ID == dd.ProductID
                    //                from mapping in mappings
                    //                where sd.UniqueCode == mapping.UniqueCode && dd.ProductID == mapping.ProductID
                    //                select dd;
                    //var goodReturnMoney = tempDeDetails.Sum(dd => dd.Price * dd.Discount);//退货总金额=唯一码对应的原发货金额
                    //if (rateitem.GoodReturnRate < (goodReturnMoney * 100 / deliveriedMoney))
                    //    return new OPResult { IsSucceed = false, Message = string.Format("无法退货,{0}年第{1}季货品超出退货率", rateitem.Year, rateitem.Quarter) };
                    //returnMoney += goodReturnMoney;
                    #endregion

                    IQueryable<BillGoodReturnDetails> tempReDetails = from goodreturn in goodreturns
                                                                      from rd in reDetails
                                                                      where goodreturn.ID == rd.BillID && goodreturn.OrganizationID == organizationID && !goodreturn.IsDefective && goodreturn.Status != (int)BillGoodReturnStatusEnum.被退回 && goodreturn.Status != (int)BillGoodReturnStatusEnum.退回已入库
                                                                      from product in tempProducts
                                                                      where product.ID == rd.ProductID
                                                                      select rd;
                    var goodReturnMoney = tempReDetails.Sum(dd => dd.Quantity * dd.Price * dd.Discount);//已退货金额
                    var nowReturnMoney = this.GridDataItems.Where(o => o.Year == rateitem.Year && o.Quarter == rateitem.Quarter).Sum(o => o.Quantity * o.Price * o.Discount);//本单退货金额

                    if (rateitem.GoodReturnRate < ((goodReturnMoney + nowReturnMoney) * 100 / deliveriedMoney))
                        return new OPResult { IsSucceed = false, Message = string.Format("无法退货,{0}年第{1}季货品超出退货率", rateitem.Year, rateitem.Quarter) };
                }
            }
            return new OPResult { IsSucceed = true };
        }
    }

    public class GoodReturnProductShow : DistributionProductShow
    {
        public override decimal Price
        {
            get
            {
                return base.Price;
            }
            set
            {
                base.Price = value;
                OnPropertyChanged("Price");
            }
        }
    }
}
