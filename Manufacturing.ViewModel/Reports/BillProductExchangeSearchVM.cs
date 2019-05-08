using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using ManufacturingModel;
using SysProcessModel;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class BillProductExchangeSearchVM : BillPagedReportVM<BillProductExchangeSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "交接品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "交接状态", PropertyName = "Status", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "单据状态", PropertyName = "IsDeleted", PropertyType = typeof(bool) },
                        new ItemPropertyDefinition { DisplayName = "生产工厂", PropertyName = "OuterFactoryID", PropertyType = typeof(int) }
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

        protected override IEnumerable<BillProductExchangeSearchEntity> SearchData()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var productExchangeContext = lp.GetDataContext<BillProductExchange>();
            var userContext = lp.GetDataContext<ViewUser>();
            var factoryContext = lp.GetDataContext<Factory>();

            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);

            var billData = from productExchange in productExchangeContext
                           from user in userContext
                           where productExchange.CreatorID == user.ID && brandIDs.Contains(productExchange.BrandID)
                           from factory in factoryContext
                           where productExchange.OuterFactoryID == factory.ID
                           select new BillProductExchangeSearchEntity
                           {
                               CreateDate = productExchange.CreateTime.Date,
                               CreateTime = productExchange.CreateTime,
                               OuterFactoryID = factory.ID,
                               OuterFactoryName = factory.Name,
                               Status = productExchange.Status,
                               ID = productExchange.ID,
                               BrandID = productExchange.BrandID,
                               Code = productExchange.Code,
                               CreatorName = user.Name,
                               Remark = productExchange.Remark,
                               IsDeleted = productExchange.IsDeleted
                           };
            var productExchangeDetailsContext = lp.GetDataContext<BillProductExchangeDetails>();
            var pids = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pids != null)
            {
                if (pids.Count() == 0)
                    return null;
                billData = from d in billData
                           where productExchangeDetailsContext.Any(sd => sd.BillID == d.ID && pids.Contains(sd.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<BillProductExchangeSearchEntity>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            var datas = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = datas.Select(o => (int)o.ID);
            var sum = productExchangeDetailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, TotalQuantity = g.Sum(o => o.Quantity) }).ToList();
            datas.ForEach(o =>
            {
                o.BrandName = brands.Find(b => b.ID == o.BrandID).Name;
                o.Quantity = sum.Find(s => s.BillID == o.ID).TotalQuantity;
            });
            return datas;
        }
    }
}
