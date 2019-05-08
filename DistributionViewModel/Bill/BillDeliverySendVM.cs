using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using DistributionModel;
using SysProcessModel;
using Kernel;
using System.Transactions;
using System.Collections.ObjectModel;
using DomainLogicEncap;
using ViewModelBasic;
using SysProcessViewModel;
using IWCFServiceForIM;
using Manufacturing.ViewModel;
using ManufacturingModel;
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillDeliverySendVM : CommonViewModel<DeliverySearchEntity>
    {
        FloatPriceHelper _fpHelper;

        public BillDeliverySendVM()
        {
            this.Entities = this.SearchData();
        }

        protected override IEnumerable<DeliverySearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = childOrganizations.Select(o => o.ID).ToArray();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            int status = (int)BillDeliveryStatusEnum.已装箱未配送;
            var deliveryContext = lp.Search<BillDelivery>(o => o.Status == status && o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var orgContext = lp.Search<ViewOrganization>(o => oids.Contains(o.ID) && o.Flag);
            var userContext = lp.GetDataContext<ViewUser>();
            var storageContext = lp.GetDataContext<Storage>();

            var billData = from d in deliveryContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           from org in orgContext
                           where org.ID == d.ToOrganizationID
                           from storage in storageContext
                           where storage.ID == d.StorageID
                           select new DeliverySearchEntity
                           {
                               CreatorID = d.CreatorID,
                               OrganizationID = d.OrganizationID,
                               ToOrganizationID = d.ToOrganizationID,//跟查询条件相关的属性需要显式声明[赋值]，即使父类里已经定义，否则在生成SQL语句的过程中会报错
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime,
                               CreatorName = user.Name,
                               ToOrganizationName = org.Name,
                               Status = d.Status,
                               StorageName = storage.Name,
                               StorageID = d.StorageID,
                               IsWriteDownOrder = d.IsWriteDownOrder
                           };
            var detailsContext = lp.GetDataContext<BillDeliveryDetails>();
            var deliveries = billData.ToList();
            var bIDs = deliveries.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity), TotalPrice = g.Sum(o => o.Price * o.Quantity), TotalCostPrice = g.Sum(o => o.Price * o.Quantity * o.Discount) / 100 }).ToList();
            deliveries.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(o => d.BrandID == o.ID).Name;
                var details = sum.Find(o => o.BillID == d.ID);
                d.Quantity = details.Quantity;
                d.TotalPrice = details.TotalPrice;
                d.TotalCostMoney = details.TotalCostPrice;
            });
            return new ObservableCollection<DeliverySearchEntity>(deliveries.OrderBy(o => o.CreateTime));
        }

        public OPResult Delete(DeliverySearchEntity entity)
        {
            var result = VMGlobal.DistributionQuery.DB.ExecuteScalar("SendbackDeliveryPackage", entity.ID, entity.ToOrganizationID, entity.IsWriteDownOrder).ToString();
            if (!string.IsNullOrEmpty(result))
                return new OPResult { IsSucceed = false, Message = "作废失败,失败原因:\n" + result };
            ((ObservableCollection<DeliverySearchEntity>)this.Entities).Remove(entity);
            return new OPResult { IsSucceed = true, Message = "作废成功." };
        }

        public OPResult Send(DeliverySearchEntity entity)
        {
            if (entity.Status != (int)BillDeliveryStatusEnum.已装箱未配送)
            {
                return new OPResult { IsSucceed = false, Message = "该单已配送" };
            }
            var bill = VMGlobal.DistributionQuery.LinqOP.GetById<BillDelivery>(entity.ID);
            if (bill.Status != (int)BillDeliveryStatusEnum.已装箱未配送)
            {
                return new OPResult { IsSucceed = false, Message = "该单已由他人配送" };
            }
            OrganizationFundAccount fundAccount = null;
            bool isSelfShop = OrganizationListVM.IsSelfRunShop(entity.ToOrganizationID);//是否自营店
            if (!isSelfShop)
            {
                var result = this.CheckFundSatisfyDelivery(entity);
                if (!result.IsSucceed)
                {
                    return result;
                }
                var totalMoney = GetTotalMoney(entity);
                fundAccount = new OrganizationFundAccount
                {
                    BrandID = entity.BrandID,
                    OrganizationID = entity.ToOrganizationID,
                    NeedIn = totalMoney,
                    AlreadyIn = 0.0M,
                    CreatorID = VMGlobal.CurrentUser.ID,
                    BillKind = (int)BillTypeEnum.BillDelivery,
                    Remark = "发货单生成",
                    RefrenceBillCode = entity.Code
                };
            }
            //生成出库单
            var storeout = this.GenerateStoreOut(entity);
            entity.Status = (int)BillDeliveryStatusEnum.在途中;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {                    
                    if (fundAccount != null)
                        VMGlobal.DistributionQuery.LinqOP.Add<OrganizationFundAccount>(fundAccount);
                    VMGlobal.DistributionQuery.LinqOP.Update<BillDelivery>(entity);
                    storeout.SaveWithNoTran();
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "配送失败\n失败原因:" + e.Message };
                }
            }
            ((ObservableCollection<DeliverySearchEntity>)this.Entities).Remove(entity);

            var users = IMHelper.OnlineUsers.Where(o => o.OrganizationID == entity.ToOrganizationID || o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToArray();
            var toName = VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(entity.ToOrganizationID).Name;
            IMHelper.AsyncSendMessageTo(users, new IMessage
            {
                Message = string.Format("发往{2}{0}件,单号{1},到货后请及时入库.", entity.Quantity, entity.Code, toName),
                Sender = IMHelper.CurrentUser
            }, IMReceiveAccessEnum.发货单);

            return new OPResult { IsSucceed = true, Message = "配送成功." };
        }

        /// <summary>
        /// 生成出库单
        /// </summary>
        private BillStoreOutVM GenerateStoreOut(DeliverySearchEntity entity)
        {
            BillStoreOutVM storeout = new BillStoreOutVM();
            var soMaster = storeout.Master;
            int typeid = (int)Enum.Parse(typeof(BillTypeEnum), typeof(BillDelivery).Name);
            soMaster.Remark = "发货出库";
            soMaster.BillType = typeid;
            soMaster.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            soMaster.RefrenceBillCode = entity.Code;
            soMaster.StorageID = entity.StorageID;
            soMaster.BrandID = entity.BrandID;
            List<BillStoreOutDetails> soDetails = new List<BillStoreOutDetails>();
            foreach (var p in entity.Details)
            {
                soDetails.Add(new BillStoreOutDetails
                {
                    ProductID = p.ProductID,
                    Quantity = p.Quantity
                });
            };
            storeout.Details = soDetails;
            return storeout;
        }

        private decimal GetTotalMoney(DeliverySearchEntity entity)
        {
            decimal totalMoney = 0.0M;
            foreach (var p in entity.Details)
            {
                totalMoney += (p.Quantity * p.Price * p.Discount);
            }
            return totalMoney / 100;
        }

        /// <summary>
        /// 检查下级机构资金和资信余额是否满足发货要求
        /// </summary>
        private OPResult CheckFundSatisfyDelivery(DeliverySearchEntity entity)
        {
            
            return new OPResult { IsSucceed = true };
        }

        public List<CertificationBO> GetCertifications(DeliverySearchEntity entity, CertificationMakeVM certvm)
        {
            if (_fpHelper == null)
                _fpHelper = new FloatPriceHelper();

            IEnumerable<string> scodes = entity.Details.Select(o => o.StyleCode);
            var styles = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle>(o => scodes.Contains(o.Code)).Select(o => new ProStyleBO(o)).ToList();
            IEnumerable<int> sids = styles.Select(o => o.ID);
            var certs = VMGlobal.SysProcessQuery.LinqOP.Search<Certification>(o => sids.Contains(o.StyleID)).Select(o => new CertificationBO(o)).ToList();
            foreach (var cert in certs)
            {
                cert.GradeName = certvm.Grades.First(o => o.ID == cert.Grade).Name;
                cert.SafetyTechniqueName = certvm.SafetyTechs.First(o => o.ID == cert.SafetyTechnique).Name;
                cert.CarriedStandardName = certvm.CarriedStandards.First(o => o.ID == cert.CarriedStandard).Name;
                var style = styles.Find(o => o.ID == cert.StyleID);
                cert.Style = style;
                cert.Price = _fpHelper.GetFloatPrice(entity.ToOrganizationID, style.BYQID, style.Price);
            }
            return certs;
        }
    }
}
