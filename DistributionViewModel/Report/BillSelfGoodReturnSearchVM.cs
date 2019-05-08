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
    public class BillSelfGoodReturnSearchVM : BillPagedReportVM<BillGoodReturnForSearch>
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
                        new ItemPropertyDefinition { DisplayName = "退货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
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

        protected override IEnumerable<BillGoodReturnForSearch> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            var deliveryContext = lp.Search<BillGoodReturn>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(o.BrandID));
            var userContext = lp.GetDataContext<ViewUser>();
            var storageContext = lp.GetDataContext<Storage>();
            var billData = from d in deliveryContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           join storage in storageContext on d.StorageID equals storage.ID into storages
                           from s in storages.DefaultIfEmpty()
                           select new BillGoodReturnForSearch
                           {
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               BrandID = d.BrandID,
                               CreateTime = d.CreateTime,
                               CreateDate = d.CreateTime.Date,
                               CreatorName = user.Name,
                               Status = d.Status,
                               StorageName = s.Name,
                               StorageID = d.StorageID,
                               TotalPrice = d.TotalPrice,
                               Quantity = d.Quantity
                           };
            var detailsContext = lp.GetDataContext<BillGoodReturnDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<BillGoodReturnForSearch>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            var goodreturns = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = goodreturns.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            goodreturns.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(o => d.BrandID == o.ID).Name;
                //var details = sum.Find(o => o.BillID == d.ID);
                //d.Quantity = details.Quantity;
            });
            return goodreturns;
        }
    }
}
