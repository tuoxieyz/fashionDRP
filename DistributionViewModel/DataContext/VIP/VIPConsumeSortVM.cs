using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using SysProcessViewModel;
using DistributionModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class VIPConsumeSortVM : CommonViewModel<VIPConsumeEntity>
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "消费日期", PropertyName = "ConsumeDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "消费品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "消费机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("ConsumeDate", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("ConsumeDate", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date)
                    };
                }
                return _filterDescriptors;
            }
        }

        public VIPConsumeSortVM()
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<VIPConsumeEntity> SearchData()
        {
            var bids = VMGlobal.PoweredBrands.Select(o => o.ID);
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var vips = lp.GetDataContext<VIPCard>();
            var retails = lp.GetDataContext<BillRetail>();
            var reDetails = lp.GetDataContext<BillRetailDetails>();
            var organizations = lp.GetDataContext<ViewOrganization>();

            IQueryable<Product> products = VMGlobal.DistributionQuery.QueryProvider.GetTable<Product>("SysProcess.dbo.Product");
            IQueryable<ProStyle> prostyles = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProStyle>("SysProcess.dbo.ProStyle");
            IQueryable<ProBYQ> byqs = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProBYQ>("SysProcess.dbo.ProBYQ");

            var data = from retail in retails
                       from rd in reDetails
                       where retail.ID == rd.BillID
                       from vip in vips
                       where retail.VIPID == vip.ID
                       from product in products
                       where rd.ProductID == product.ID
                       from prostyle in prostyles
                       where product.StyleID == prostyle.ID
                       from byq in byqs
                       where byq.ID == prostyle.BYQID && bids.Contains(byq.BrandID)
                       from organization in organizations
                       where vip.OrganizationID == organization.ID
                       select new VIPConsumeForSearchEntity
                       {
                           RetailID = retail.ID,
                           ConsumeDate = retail.CreateTime.Date,
                           BrandID = byq.BrandID,
                           OrganizationID = retail.OrganizationID,
                           VIPID = vip.ID,
                           VIPCode = vip.Code,
                           VIPName = vip.CustomerName,
                           VIPSendOrganizationName = organization.Name,
                           ConsumeMoney = rd.Quantity * rd.Price * rd.Discount
                       };
            var filtedData = (IQueryable<VIPConsumeForSearchEntity>)data.Where(FilterDescriptors);
            var temp = filtedData.ToList();
            var result = temp.GroupBy(o => new { o.VIPID, o.VIPCode, o.VIPName, o.VIPSendOrganizationName }).Select(g => new VIPConsumeEntity
            {
                VIPID = g.Key.VIPID,
                VIPCode = g.Key.VIPCode,
                VIPName = g.Key.VIPName,
                VIPSendOrganizationName = g.Key.VIPSendOrganizationName,
                ConsumeMoney = (g.Sum(o => o.ConsumeMoney) / 100)
            }).ToList();
            result.ForEach(o =>
            {
                o.ConsumeTimes = temp.Where(t => t.VIPID == o.VIPID).Select(t => t.RetailID).Distinct().Count();
            });
            ApplyVIPKind(result);
            return result.OrderByDescending(o => o.ConsumeMoney);
        }

        private void ApplyVIPKind(List<VIPConsumeEntity> vips)
        {
            var ids = vips.Select(o => o.VIPID).ToArray();
            var vks = VMGlobal.DistributionQuery.LinqOP.Search<VIPCardKindMapping>(o => ids.Contains(o.CardID)).ToList();
            var kids = vks.Select(o => o.KindID).Distinct().ToArray();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var ks = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(o => kids.Contains(o.ID) && brandIDs.Contains(o.BrandID)).ToList();
            vips.ForEach(o =>
            {
                var data = from vk in vks
                           from k in ks
                           where vk.KindID == k.ID && vk.CardID == o.VIPID
                           select k;
                o.Kinds = data.ToList();
            });
        }

        private class VIPConsumeForSearchEntity : VIPConsumeEntity
        {
            public int RetailID { get; set; }
            public DateTime ConsumeDate { get; set; }
            public int BrandID { get; set; }
            public int OrganizationID { get; set; }
        }
    }

    public class VIPConsumeEntity
    {
        public int VIPID { get; set; }
        public string VIPCode { get; set; }
        public string VIPName { get; set; }
        /// <summary>
        /// 开卡店铺
        /// </summary>
        public string VIPSendOrganizationName { get; set; }
        public decimal ConsumeMoney { get; set; }
        /// <summary>
        /// 消费次数
        /// </summary>
        public int ConsumeTimes { get; set; }

        public List<VIPKind> Kinds { get; set; }
    }
}
