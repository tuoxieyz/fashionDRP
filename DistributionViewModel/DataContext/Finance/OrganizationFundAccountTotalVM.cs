using DistributionModel;
using DistributionModel.Finance;
using ERPModelBO;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class OrganizationFundAccountTotalVM : CommonViewModel<FundAccountTotalEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public int BrandID { get; set; }

        protected override IEnumerable<FundAccountTotalEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var fundAccountContext = lp.Search<OrganizationFundAccount>(o => oids.Contains(o.OrganizationID));
            if (BrandID != default(int))
                fundAccountContext = fundAccountContext.Where(o => o.BrandID == BrandID);
            var temp = fundAccountContext.GroupBy(o => new { o.OrganizationID, o.BrandID }).Select(g => new { g.Key, NeedIn = g.Sum(o => o.NeedIn), AlreadyIn = g.Sum(o => o.AlreadyIn) }).ToList();
            return temp.Select(o => new FundAccountTotalEntity
            {
                OrganizationID = o.Key.OrganizationID,
                BrandID = o.Key.BrandID,
                BrandName = VMGlobal.PoweredBrands.Find(b => b.ID == o.Key.BrandID).Name,
                OrganizationCode = OrganizationArray.First(b => b.ID == o.Key.OrganizationID).Code,
                OrganizationName = OrganizationArray.First(b => b.ID == o.Key.OrganizationID).Name,
                AlreadyIn = o.AlreadyIn,
                NeedIn = o.NeedIn,
                Balance = o.AlreadyIn - o.NeedIn
            }).ToList();
        }

        /// <summary>
        /// 获取可用余额
        /// </summary>
        internal static decimal GetAvailableBalance(int organizationID, int brandID)
        {
            var balanceQuery = VMGlobal.DistributionQuery.LinqOP.Search<OrganizationFundAccount>(o => o.OrganizationID == organizationID && o.BrandID == brandID);
            var balance = balanceQuery.Sum(o => o.AlreadyIn - o.NeedIn);
            var frozenMoeny = VMGlobal.DistributionQuery.LinqOP.Search<VoucherReceiveMoney>(o => o.OrganizationID == organizationID && o.BrandID == brandID && o.IsMoneyFrozen && o.Status).Sum(o => o.ReceiveMoney);
            return balance - frozenMoeny;
        }

        public List<FundAccountSearchEntity> SearchFundAccount(int organizationID, int brandID, int pageIndex, int pageSize, ref int totalCount)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var fundAccountContext = lp.Search<OrganizationFundAccount>(o => o.OrganizationID == organizationID && o.BrandID == brandID);
            var orgContext = lp.Search<ViewOrganization>(o => o.Flag);
            var brands = VMGlobal.PoweredBrands;
            var data = from fa in fundAccountContext
                       from org in orgContext
                       where org.ID == fa.OrganizationID
                       select new FundAccountSearchEntity
                       {
                           OrganizationID = fa.OrganizationID,
                           BrandID = fa.BrandID,
                           AlreadyIn = fa.AlreadyIn,
                           NeedIn = fa.NeedIn,
                           OccurDate = fa.CreateTime.Date,
                           CreateTime = fa.CreateTime,
                           OrganizationCode = org.Code,
                           OrganizationName = org.Name,
                           RefrenceBillCode = fa.RefrenceBillCode,
                           RefrenceBillKind = Enum.GetName(typeof(BillTypeEnum), fa.BillKind),
                           Remark = fa.Remark,
                           BalanceAtThatTime = fundAccountContext.Where(o => o.OrganizationID == fa.OrganizationID && o.BrandID == fa.BrandID && o.CreateTime <= fa.CreateTime).Sum(o => o.AlreadyIn - o.NeedIn)
                       };
            totalCount = data.Count();
            var result = data.OrderByDescending(o => o.CreateTime).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            result.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(b => b.ID == d.BrandID).Name;
                var typeField = typeof(BillTypeEnum).GetField(d.RefrenceBillKind);
                var displayNames = typeField.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
                d.RefrenceBillKind = ((EnumDescriptionAttribute)displayNames[0]).Description;
            });
            return result;
        }
    }
}
