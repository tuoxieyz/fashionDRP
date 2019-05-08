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
    public class BillStocktakeSearchVM : BillPagedReportVM<StocktakeSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "盘点仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "盘点品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "库存更新状态", PropertyName = "Status", PropertyType = typeof(bool)},
                        new ItemPropertyDefinition { DisplayName = "单据状态", PropertyName = "IsDeleted", PropertyType = typeof(bool)}
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
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue),
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<StocktakeSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var stocktakeContext = lp.Search<BillStocktake, StocktakeEntityForSearch>(selector: o => new StocktakeEntityForSearch
            {
                BrandID = o.BrandID,
                Code = o.Code,
                CreateTime = o.CreateTime,
                CreateDate = o.CreateTime.Date,
                CreatorID = o.CreatorID,
                ID = o.ID,
                IsDeleted = o.IsDeleted,
                OrganizationID = o.OrganizationID,
                Remark = o.Remark,
                Status = o.Status,
                StorageID = o.StorageID
            },
            condition: o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var filtedData = (IQueryable<BillStocktake>)stocktakeContext.Where(FilterDescriptors);
            var detailsContext = lp.GetDataContext<BillStocktakeDetails>();
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
                         select new StocktakeSearchEntity
                         {
                             BillID = d.ID,
                             BrandID = d.BrandID,
                             Code = d.Code,
                             CreateTime = d.CreateTime,
                             CreatorName = user.Name,
                             Remark = d.Remark,
                             StorageName = storage.Name,
                             Status = d.Status,
                             IsDeleted = d.IsDeleted
                         };
            var stocktakes = result.ToList();
            var bIDs = stocktakes.Select(o => (int)o.BillID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            stocktakes.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(b => b.ID == d.BrandID).Name;
                var details = sum.Find(o => o.BillID == d.BillID);
                d.Quantity = details.Quantity;
            });
            return stocktakes;
        }
    }
}
