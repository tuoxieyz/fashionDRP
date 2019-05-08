using DBEncapsulation;
using ERPModelBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using DistributionModel;
using Kernel;
using System.Transactions;

namespace WEBAPI.Controllers
{
    public class BillRetailController : ApiController
    {
        [HttpPost]
        public VIPBO GetVIPInfo(string vcode, int[] brandIDs)
        {
            using (var dbContext = new DistributionEntities())
            {
                var card = dbContext.VIPCard.FirstOrDefault(v => v.Code == vcode);
                return this.ToBo(card, brandIDs, dbContext);
            }
        }

        [HttpPost]
        public VIPBO GetVIPInfo(int vid, int[] brandIDs)
        {
            using (var dbContext = new DistributionEntities())
            {
                var card = dbContext.VIPCard.FirstOrDefault(v => v.ID == vid);
                return this.ToBo(card, brandIDs, dbContext);
            }
        }

        private VIPBO ToBo(VIPCard card, int[] brandIDs, DistributionEntities dbContext)
        {
            if (card != null)
            {
                VIPBO bo = new VIPBO { CardInfo = card };
                //防止无记录时null产生的空值转int问题，所以先转成int?
                var point = dbContext.VIPPointTrack.Where(v => v.VIPID == card.ID).Sum(v => (int?)v.Point);
                bo.CardPoint = point ?? 0;
                bo.Kinds = this.GetVIPKinds(card.ID, brandIDs, dbContext);
                if (DateTime.Now.Month == card.Birthday.Month && DateTime.Now.Day == card.Birthday.Day)//当天生日
                {
                    var date = DateTime.Now.Date;
                    bo.BirthdayConsumption = dbContext.VIPBirthdayConsumption.FirstOrDefault(v => v.VIPID == card.ID && v.ConsumeDay == date);
                    bo.BirthdayConsumption = bo.BirthdayConsumption ?? new VIPBirthdayConsumption
                    {
                        VIPID = card.ID,
                        ConsumeDay = DateTime.Now.Date
                    };
                }
                return bo;
            }
            return null;
        }

        /// <summary>
        /// 根据VIPID获取对应的类型集合
        /// </summary>
        private List<VIPKind> GetVIPKinds(int vid, int[] brandIDs, DistributionEntities dbContext)
        {
            var query = from mapping in dbContext.VIPCardKindMapping
                        from kind in dbContext.VIPKind
                        where mapping.KindID == kind.ID && mapping.CardID == vid && brandIDs.Contains(kind.BrandID)
                        select kind;
            return query.ToList();
        }

        public OPResult<BillRetail> SaveBillRetail(BillRetailBO bo)
        {
            //bo.Bill.CreateTime = DateTime.Now;

            using (var dbContext = new DistributionEntities())
            {
                if (bo.RefrenseVIPUpTactics != null && bo.VIPPointRecord != null)
                {
                    IEnumerable<int> kindIDs = bo.RefrenseVIPUpTactics.Select(o => o.Tactic.FormerKindID).ToList();
                    IEnumerable<VIPCardKindMapping> mappings = dbContext.VIPCardKindMapping.Where(o => o.CardID == bo.VIPPointRecord.VIPID && kindIDs.Contains(o.KindID)).ToList();
                    foreach (var mapping in mappings)
                    {
                        var t = bo.RefrenseVIPUpTactics.First(o => o.Tactic.FormerKindID == mapping.KindID);
                        mapping.KindID = t.Tactic.AfterKindID;
                        if (t.Tactic.CutPoint != 0)
                        {
                            dbContext.VIPPointTrack.Add(new VIPPointTrack
                            {
                                CreateTime = DateTime.Now,
                                Point = (-1 * t.Tactic.CutPoint),
                                VIPID = bo.VIPPointRecord.VIPID,
                                Remark = "VIP升级产生," + t.Description
                            });
                        }
                    }
                }
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        bo.Bill.Code = BillHelper.GenerateBillCode<BillRetail>(bo.Bill.OrganizationID, dbContext);
                        BillHelper.SaveBill<BillRetail, BillRetailDetails>(bo, dbContext, bo.SpecifcCreateTime);
                        foreach (var storeout in bo.BillStoreOuts)
                        {
                            storeout.Bill.CreateTime = DateTime.Now;
                            storeout.Bill.RefrenceBillCode = bo.Bill.Code;
                            BillHelper.SaveBillStoreOut(storeout, dbContext);
                        }
                        foreach (var storing in bo.BillStorings)
                        {
                            storing.Bill.CreateTime = DateTime.Now;
                            storing.Bill.RefrenceBillCode = bo.Bill.Code;
                            BillHelper.SaveBillStoring(storing, dbContext);
                        }
                        if (bo.VIPPointRecord != null)
                        {
                            bo.VIPPointRecord.Remark = "零售单产生,小票号" + bo.Bill.Code;
                            dbContext.VIPPointTrack.Add(bo.VIPPointRecord);

                            if (bo.Bill.PredepositPay != 0)
                            {
                                VIPPredepositTrack predepositTrack = new VIPPredepositTrack
                                {
                                    RefrenceBillCode = bo.Bill.Code,
                                    Kind = false,
                                    OrganizationID = bo.Bill.OrganizationID,
                                    CreatorID = bo.Bill.CreatorID,
                                    CreateTime = DateTime.Now,
                                    ConsumeMoney = bo.Bill.PredepositPay,
                                    VIPID = bo.VIPPointRecord.VIPID,
                                    Remark = "零售单产生"
                                };
                                dbContext.VIPPredepositTrack.Add(predepositTrack);
                            }
                        }
                        if (bo.VIPBirthdayConsumption != null)
                        {
                            bo.VIPBirthdayConsumption.ConsumeDay = DateTime.Now.Date;
                            dbContext.VIPBirthdayConsumption.Add(bo.VIPBirthdayConsumption);
                        }

                        dbContext.SaveChanges();

                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        return new OPResult<BillRetail> { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                    }
                }
            }
            return new OPResult<BillRetail> { IsSucceed = true, Message = "保存成功!", Result = bo.Bill };
        }

        public OPResult<VIPPredepositTrack> SaveVIPPrestore(VIPPredepositTrack prestore)
        {
            DateTime time = DateTime.Now;
            prestore.CreateTime = time;
            string timestr = "-" + time.ToString("yyyyMMdd");
            string prefixion = "VP";

            using (var dbContext = new DistributionEntities())
            {
                try
                {
                    //if (string.IsNullOrEmpty(prestore.RefrenceBillCode))//预存金额
                    var maxCode = dbContext.VIPPredepositTrack.Where(o => o.OrganizationID == prestore.OrganizationID && o.RefrenceBillCode.StartsWith(prefixion) && o.RefrenceBillCode.Contains(timestr)).Max(t => t.RefrenceBillCode);
                    if (string.IsNullOrEmpty(maxCode))
                    {
                        var ocode = dbContext.ViewOrganization.Where(b => b.ID == prestore.OrganizationID).Select(o => o.Code).First();
                        maxCode = prefixion + ocode + timestr + "000";
                    }
                    int preLength = maxCode.Length - 3;
                    prestore.RefrenceBillCode = maxCode.Substring(0, preLength) + (Convert.ToInt32(maxCode.Substring(preLength)) + 1).ToString("000");
                    var balance = dbContext.VIPPredepositTrack.Where(o => o.VIPID == prestore.VIPID).Sum(o => (decimal?)(o.StoreMoney + o.FreeMoney - o.ConsumeMoney));
                    dbContext.VIPPredepositTrack.Add(prestore);
                    dbContext.SaveChanges();
                    return new OPResult<VIPPredepositTrack> { IsSucceed = true, Message = string.Format("保存成功, 当前余额:{0:C2}", (balance ?? 0) + prestore.StoreMoney + prestore.FreeMoney), Result = prestore };
                }
                catch (Exception e)
                {
                    return new OPResult<VIPPredepositTrack> { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
        }
    }
}
