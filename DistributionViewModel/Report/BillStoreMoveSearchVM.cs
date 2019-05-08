using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;

using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillStoreMoveSearchVM : BillPagedReportVM<StoreMoveSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "移出仓库", PropertyName = "StorageIDOut", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "移入仓库", PropertyName = "StorageIDIn", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "移库品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
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
                        new FilterDescriptor("StorageIDOut", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("StorageIDIn", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<StoreMoveSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var storemoveContext = lp.Search<BillStoreMove, BillStoreMove>(selector: o => new BillStoreMove
            {
                BrandID = o.BrandID,
                Code = o.Code,
                CreateTime = o.CreateTime.Date,
                CreatorID = o.CreatorID,
                ID = o.ID,
                OrganizationID = o.OrganizationID,
                Remark = o.Remark,
                StorageIDOut = o.StorageIDOut,
                StorageIDIn = o.StorageIDIn
            },
            condition: o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var filtedData = (IQueryable<BillStoreMove>)storemoveContext.Where(FilterDescriptors);   
            var detailsContext = lp.GetDataContext<BillStoreMoveDetails>();
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
                         select new StoreMoveSearchEntity
                         {
                             ID = d.ID,
                             BrandID = d.BrandID,
                             Code = d.Code,
                             CreateTime = d.CreateTime,
                             CreatorName = user.Name,
                             Remark = d.Remark,
                             StorageIDOut = d.StorageIDOut,
                             StorageIDIn = d.StorageIDIn
                         };
            var storemove = result.ToList();
            var bIDs = storemove.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            storemove.ForEach(d =>
            {
                d.BrandName = brands.First(b => b.ID == d.BrandID).Name;
                d.Quantity = sum.Find(o => o.BillID == d.ID).Quantity;
                d.OutStorageName = ReportDataContext.Storages.First(b => b.ID == d.StorageIDOut).Name;
                d.InStorageName = ReportDataContext.Storages.First(b => b.ID == d.StorageIDIn).Name;
            });
            return storemove;
        }
    }
}
