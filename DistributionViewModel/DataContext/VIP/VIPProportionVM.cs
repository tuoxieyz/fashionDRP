using DBAccess;
using DistributionModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;

namespace DistributionViewModel
{
    public class VIPProportionVM : ViewModelBase
    {
        public int BrandID { get; set; }

        private IEnumerable<int> _downHierarchyOrganizationIDArray;
        public IEnumerable<int> DownHierarchyOrganizationIDArray
        {
            get
            {
                if (_downHierarchyOrganizationIDArray == null)
                {
                    _downHierarchyOrganizationIDArray = OrganizationListVM.GetOrganizationDownHierarchy(VMGlobal.CurrentUser.OrganizationID);
                }
                return _downHierarchyOrganizationIDArray;
            }
        }

        public VIPProportionVM()
        {
            if (VMGlobal.PoweredBrands.Count == 1)
            {
                BrandID = VMGlobal.PoweredBrands.First().ID;
                OnPropertyChanged("BrandID");
            }
        }

        public VIPProportionVM(int brandID, int organizationID)
        {
            BrandID = brandID;
            _downHierarchyOrganizationIDArray = OrganizationListVM.GetOrganizationDownHierarchy(organizationID);
        }        

        public IEnumerable<VIPKindProportion> GetKindProportion()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var cards = lp.Search<VIPCard>(o => DownHierarchyOrganizationIDArray.Contains(o.OrganizationID));
            var maps = lp.GetDataContext<VIPCardKindMapping>();
            var kinds = lp.Search<VIPKind>(o => o.BrandID == BrandID);
            var data = from kind in kinds
                       from map in maps
                       where kind.ID == map.KindID
                       from card in cards
                       where card.ID == map.CardID
                       select new
                       {
                           KindID = kind.ID,
                           KindName = kind.Name
                       };

            var temp = data.GroupBy(o => o.KindName).Select(g => new VIPKindProportion { Name = g.Key, Quantity = g.Count() }).ToList();
            var amount = temp.Sum(o => o.Quantity);
            temp.ForEach(o =>
            {
                o.Title = string.Format("{0}: {1}", o.Name, o.Quantity);
                o.Description = string.Format("{0}%", Math.Round(o.Quantity * 100.0 / amount));
            });
            return temp;
        }

        public IEnumerable<VIPConsumeProportion> GetConsumeProportion()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var retails = lp.Search<BillRetail>(o => DownHierarchyOrganizationIDArray.Contains(o.OrganizationID));
            var maps = lp.GetDataContext<VIPCardKindMapping>();
            var kinds = lp.Search<VIPKind>(o => o.BrandID == BrandID);
            var temp = from kind in kinds
                       from map in maps
                       where kind.ID == map.KindID
                       select new { VIPID = map.CardID, KindName = kind.Name };
            var data = from retail in retails
                       join vip in temp on retail.VIPID equals vip.VIPID into vips
                       from v in vips.DefaultIfEmpty()//生成Left [outer] join左联接语句，就不会产生重复数据了 
                       select new
                       {
                           CostMoney = retail.CostMoney,
                           KindName = v.KindName
                       };

            var result = data.GroupBy(o => o.KindName).Select(g => new VIPConsumeProportion { Name = g.Key, ConsumeMoney = g.Sum(o => o.CostMoney) }).ToList();
            var amount = result.Sum(o => o.ConsumeMoney);
            result.ForEach(o =>
            {
                if (string.IsNullOrEmpty(o.Name))
                    o.Name = "非VIP";
                o.Title = string.Format("{0}: {1:C}", o.Name, o.ConsumeMoney);
                o.Description = string.Format("{0}%", Math.Round(o.ConsumeMoney * 100 / amount));
            });
            return result;
        }

        public IEnumerable<VIPActiveProportion> GetActiveProportion()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var cards = lp.Search<VIPCard>(o => DownHierarchyOrganizationIDArray.Contains(o.OrganizationID));
            var maps = lp.GetDataContext<VIPCardKindMapping>();
            var kinds = lp.Search<VIPKind>(o => o.BrandID == BrandID);
            var cardIDs = (from card in cards
                          from map in maps
                          where map.CardID == card.ID
                          from kind in kinds
                          where kind.ID == map.KindID
                          select card.ID).Distinct();

            var retails = lp.GetDataContext<BillRetail>();
            var data = from retail in retails
                       from cardID in cardIDs
                       where retail.VIPID == cardID && retail.CreateTime > DateTime.Now.AddMonths(-9)
                       select new
                       {
                           VIPID = cardID,
                           RetailTime = retail.CreateTime
                       };

            var temp = data.ToList();
            var amount = cardIDs.Count();
            //var temp3 = temp.FindAll(o => o.RetailTime > DateTime.Now.AddMonths(-3)).GroupBy(o => o.VIPID).Select(g => new { VIPID = g.Key, Quantity = g.Count() }).ToList();
            var newvips = cards.Where(o => o.CreateTime > DateTime.Now.AddMonths(-3)).Select(o => o.ID).Distinct().ToArray();
            VIPActiveProportion newVIP = new VIPActiveProportion
            {
                Name = "新入VIP",
                Title = "新入VIP: 近3月内新办卡",
                Quantity = newvips.Count()
            };
            var activevips = temp.FindAll(o => o.RetailTime > DateTime.Now.AddMonths(-3) && !newvips.Contains(o.VIPID)).Select(o => o.VIPID).Distinct().ToArray();
            VIPActiveProportion activeVIP = new VIPActiveProportion
            {
                Name = "活跃VIP",
                Title = "活跃VIP: 近3月内有消费,不包括新入VIP",
                Quantity = activevips.Count()
            };
            var edgevips = temp.FindAll(o => o.RetailTime <= DateTime.Now.AddMonths(-3) && o.RetailTime >= DateTime.Now.AddMonths(-6) && !newvips.Contains(o.VIPID) && !activevips.Contains(o.VIPID)).Select(o => o.VIPID).Distinct().ToArray();
            VIPActiveProportion edgeVIP = new VIPActiveProportion
            {
                Name = "边缘VIP",
                Title = "边缘VIP: 半年内有消费,但近3月内无消费",
                Quantity = edgevips.Count()
            };
            var sleepvips = temp.FindAll(o => o.RetailTime < DateTime.Now.AddMonths(-6) && o.RetailTime >= DateTime.Now.AddMonths(-9) && !newvips.Contains(o.VIPID) && !activevips.Contains(o.VIPID) && !edgevips.Contains(o.VIPID)).Select(o => o.VIPID).Distinct().ToArray();
            VIPActiveProportion sleepVIP = new VIPActiveProportion
            {
                Name = "沉睡VIP",
                Title = "沉睡VIP: 近9月内有消费,但半年内无消费",
                Quantity = sleepvips.Count()
            };
            VIPActiveProportion awayVIP = new VIPActiveProportion
            {
                Name = "流失VIP",
                Title = "流失VIP: 近9月内无消费",
                Quantity = amount - (newVIP.Quantity + activeVIP.Quantity + edgeVIP.Quantity + sleepVIP.Quantity)
            };

            var result = (new VIPActiveProportion[] { newVIP, activeVIP, edgeVIP, sleepVIP, awayVIP }).Where(o => o.Quantity != 0).ToList();
            foreach (var o in result)
            {
                o.Description = string.Format("{0}%", Math.Round(o.Quantity * 100.0 / amount));
            }
            return result;
        }
    }

    public class VIPKindProportion
    {
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }

    public class VIPConsumeProportion
    {
        public decimal ConsumeMoney { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }

    public class VIPActiveProportion
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
