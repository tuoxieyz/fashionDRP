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
    public class BillStocktakeContrastSearchVM : BillPagedReportVM<ContrastSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "产生日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "更新仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "更新品牌", PropertyName = "BrandID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<ContrastSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var contrastContext = lp.Search<BillStocktakeContrast, BillStocktakeContrast>(selector: o => new BillStocktakeContrast
            {
                BrandID = o.BrandID,
                Code = o.Code,
                CreateTime = o.CreateTime.Date,
                CreatorID = o.CreatorID,
                ID = o.ID,
                OrganizationID = o.OrganizationID,
                Remark = o.Remark,
                StorageID = o.StorageID
            },
            condition: o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var filtedData = (IQueryable<BillStocktakeContrast>)contrastContext.Where(FilterDescriptors);    
            var detailsContext = lp.GetDataContext<BillStocktakeContrastDetails>();
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
            var storageContext = lp.GetDataContext<Storage>();
            var result = from d in filtedData
                         from user in userContext
                         where d.CreatorID == user.ID
                         from storage in storageContext
                         where d.StorageID == storage.ID
                         select new ContrastSearchEntity
                         {
                             ID = d.ID,
                             BrandID = d.BrandID,
                             Code = d.Code,
                             CreateTime = d.CreateTime,
                             CreatorName = user.Name,
                             Remark = d.Remark,
                             StorageName = storage.Name
                         };
            var contrasts = result.ToList();
            var bIDs = contrasts.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new
            {
                BillID = g.Key,
                Quantity = g.Sum(o => o.Quantity),
                QuaStocktake = g.Sum(o => o.QuaStocktake),
                QuaStockOrig = g.Sum(o => o.QuaStockOrig),
                QuaContrast = g.Sum(o => Math.Abs(o.QuaStocktake - o.QuaStockOrig))
            }).ToList();
            contrasts.ForEach(d =>
            {
                d.BrandName = brands.First(b => b.ID == d.BrandID).Name;
                var details = sum.Find(o => o.BillID == d.ID);
                d.Quantity = details.Quantity;
                d.QuaStockOrig = details.QuaStockOrig;
                d.QuaStocktake = details.QuaStocktake;
                d.QuaContrast = details.QuaContrast;
            });
            return contrasts;
        }
    }
}
