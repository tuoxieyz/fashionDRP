using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DistributionModel;
using DomainLogicEncap;
using SysProcessModel;
using System.Windows.Input;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillDeliverySearchVM : BillPagedReportVM<DeliverySearchEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

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
                        new ItemPropertyDefinition { DisplayName = "发货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(int) }
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

        protected override IEnumerable<DeliverySearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var deliveryContext = lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && oids.Contains(o.ToOrganizationID) && brandIDs.Contains(o.BrandID));
            //var orgContext = lp.Search<ViewOrganization>(o => oids.Contains(o.ID) && o.Flag);
            var userContext = lp.GetDataContext<ViewUser>();
            var storageContext = lp.GetDataContext<Storage>();
            
            var billData = from d in deliveryContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           from storage in storageContext
                           where storage.ID == d.StorageID
                           select new DeliverySearchEntity
                           {
                               ToOrganizationID = d.ToOrganizationID,//跟查询条件相关的属性需要显式声明[赋值]，即使父类里已经定义，否则在生成SQL语句的过程中会报错
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime,
                               CreateDate = d.CreateTime.Date,
                               CreatorName = user.Name,
                               Status = d.Status,
                               StorageName = storage.Name,
                               StorageID = d.StorageID,
                               DeliveryKind = d.DeliveryKind
                           };
            var detailsContext = lp.GetDataContext<BillDeliveryDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<DeliverySearchEntity>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            var deliveries = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = deliveries.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity), TotalPrice = g.Sum(o => o.Price * o.Quantity), TotalCostPrice = g.Sum(o => o.Price * o.Quantity * o.Discount) }).ToList();
            deliveries.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(o => d.BrandID == o.ID).Code;
                var details = sum.Find(o => o.BillID == d.ID);
                d.Quantity = details.Quantity;
                d.TotalPrice = details.TotalPrice;
                d.TotalCostMoney = details.TotalCostPrice * (0.01m);

                d.ToOrganizationName = VMGlobal.ChildOrganizations.Find(o => o.ID == d.ToOrganizationID).Name;
            });
            return deliveries;
        }
    }
}
