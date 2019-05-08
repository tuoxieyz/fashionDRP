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
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillStoreOutSearchVM : BillPagedReportVM<StoreOutSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},                
                        new ItemPropertyDefinition { DisplayName = "出库仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "出库品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)}
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
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<StoreOutSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var storeoutContext = lp.Search<BillStoreOut, BillStoreOut>(selector: o => new BillStoreOut
            {
                BrandID = o.BrandID,
                Code = o.Code,
                CreateTime = o.CreateTime.Date,
                CreatorID = o.CreatorID,
                ID = o.ID,
                OrganizationID = o.OrganizationID,
                Remark = o.Remark,
                StorageID = o.StorageID,
                BillType = o.BillType,
                RefrenceBillCode = o.RefrenceBillCode
            },
            condition: o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var filtedData = (IQueryable<BillStoreOut>)storeoutContext.Where(FilterDescriptors);
            var detailsContext = lp.GetDataContext<BillStoreOutDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                filtedData = from d in filtedData
                             where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                             select d;
            }
            TotalCount = filtedData.Count();
            filtedData = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize);
            var userContext = lp.GetDataContext<ViewUser>();
            var result = from d in filtedData
                         from user in userContext
                         where d.CreatorID == user.ID
                         select new StoreOutSearchEntity
                         {
                             ID = d.ID,
                             BrandID = d.BrandID,
                             Code = d.Code,
                             CreateTime = d.CreateTime,
                             CreatorName = user.Name,
                             Remark = d.Remark,
                             StorageID = d.StorageID,
                             StoreOutType = Enum.GetName(typeof(BillStoreOutTypeEnum), d.BillType),
                             RefrenceBillCode = d.RefrenceBillCode
                         };
            var storeouts = result.ToList();
            var bIDs = storeouts.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            storeouts.ForEach(d =>
            {
                d.BrandName = brands.First(b => b.ID == d.BrandID).Name;
                d.StorageName = ReportDataContext.Storages.Find(s => s.ID == d.StorageID).Name;
                d.Quantity = sum.Find(o => o.BillID == d.ID).Quantity;
            });
            return storeouts;
        }
    }
}
