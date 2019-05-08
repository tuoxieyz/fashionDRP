using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DomainLogicEncap;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillSubordinateStocktakeSearchVM : BillPagedReportVM<StocktakeSearchEntity>
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
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<StocktakeSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var stocktakeContext = lp.Search<BillStocktake>(o => oids.Contains(o.OrganizationID) && brandIDs.Contains(o.BrandID)); ;            
            var billData = from o in stocktakeContext
                           select new StocktakeEntityForSearch
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
                               Status = o.Status
                           };
            var detailsContext = lp.GetDataContext<BillStocktakeDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<StocktakeEntityForSearch>)billData.Where(FilterDescriptors);

            TotalCount = filtedData.Count();
            filtedData = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize);
            var userContext = lp.GetDataContext<ViewUser>();
            var result = from d in filtedData
                         from user in userContext
                         where d.CreatorID == user.ID
                         select new StocktakeSearchEntity
                         {
                             BillID = d.ID,
                             BrandID = d.BrandID,
                             Code = d.Code,
                             CreateTime = d.CreateTime,
                             CreatorName = user.Name,
                             Remark = d.Remark,
                             Status = d.Status,
                             IsDeleted = d.IsDeleted,
                             OrganizationID = d.OrganizationID
                         };
            var stocktakes = result.ToList();
            var bIDs = stocktakes.Select(o => (int)o.BillID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            stocktakes.ForEach(d =>
            {
                d.OrganizationName = OrganizationArray.First(o => o.ID == d.OrganizationID).Name;
                d.BrandName = brands.FirstOrDefault(b => b.ID == d.BrandID).Name;
                var details = sum.Find(o => o.BillID == d.BillID);
                d.Quantity = details.Quantity;
            });
            return stocktakes;
        }
    }
}
