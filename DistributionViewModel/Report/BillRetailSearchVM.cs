using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillRetailSearchVM : BillPagedReportVM<RetailSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "班次", PropertyName = "ShiftID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "导购", PropertyName = "GuideID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "VIP卡号", PropertyName = "VIPCode", PropertyType = typeof(string) }
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
                        new FilterDescriptor("CreateDate", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateDate", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<RetailSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var retailContext = lp.Search<BillRetail>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            var userContext = lp.GetDataContext<ViewUser>();
            var storageContext = lp.GetDataContext<Storage>();
            var vipContext = lp.GetDataContext<VIPCard>();
            var guideContext = lp.GetDataContext<RetailShoppingGuide>();
            var shiftContext = lp.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);

            var billData = from d in retailContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           from storage in storageContext
                           where storage.ID == d.StorageID
                           //from vip in vipContext
                           //where (d.VIPID == vip.ID || d.VIPID == null) //可能会产生重复数据
                           join vip in vipContext on d.VIPID equals vip.ID into vips
                           from v in vips.DefaultIfEmpty()//生成Left [outer] join左联接语句，就不会产生重复数据了 
                           join guide in guideContext on d.GuideID equals guide.ID into guides
                           from g in guides.DefaultIfEmpty()
                           join shift in shiftContext on d.ShiftID equals shift.ID into shifts
                           from s in shifts.DefaultIfEmpty()
                           select new RetailSearchEntity
                           {
                               OrganizationID = d.OrganizationID,//跟查询条件相关的属性需要显式声明[赋值]，即使父类里已经定义，否则在生成SQL语句的过程中会报错
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               CreateDate = d.CreateTime.Date,
                               CreateTime = d.CreateTime,
                               CreatorName = user.Name,
                               StorageName = storage.Name,
                               StorageID = d.StorageID,
                               VIPID = d.VIPID,
                               //VIPCode = (d.VIPID == null ? "" : vipContext.First(v => v.ID == d.VIPID).Code),//这样是可以滴
                               VIPCode = v.Code,
                               VIPName = v.CustomerName,
                               ShiftID = d.ShiftID,
                               ShiftName = s.Name,
                               GuideID = d.GuideID,
                               GuideName = g.Name,
                               Quantity = d.Quantity,
                               CostMoney = d.CostMoney,
                               ReceiveMoney = d.ReceiveMoney,
                               TicketMoney = d.TicketMoney,
                               PredepositPay = d.PredepositPay
                           };
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<RetailSearchEntity>)billData.Where(FilterDescriptors);
            filtedData = filtedData.Distinct();
            TotalCount = filtedData.Count();
            if (TotalCount == 0)
                return null;
            var retails = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            //var vipIDs = retails.Where(o => o.VIPID != null).Select(o => o.VIPID.Value);
            //var guideIDs = retails.Where(o => o.GuideID != null).Select(o => o.GuideID.Value);
            //var vips = lp.Search<VIPCard>(o => vipIDs.Contains(o.ID)).ToList();
            //var guides = lp.Search<RetailShoppingGuide>(o => guideIDs.Contains(o.ID)).ToList();
            //retails.ForEach(o =>
            //{
            //    if (o.VIPID != null)
            //        o.VIPName = vips.Find(v => v.ID == o.VIPID).CustomerName;
            //    if (o.GuideID != null)
            //        o.GuideName = guides.Find(g => g.ID == o.GuideID).Name;
            //});
            //var bIDs = retails.Select(o => (int)o.ID);
            //var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity), TotalPrice = g.Sum(o => o.Price * o.Quantity * o.Discount/100.0M) }).ToList();
            //retails.ForEach(d =>
            //{
            //    var details = sum.Find(o => o.BillID == d.ID);
            //    d.Quantity = details.Quantity;
            //    d.TotalPrice = details.TotalPrice;
            //});
            return retails;
        }
    }
}
