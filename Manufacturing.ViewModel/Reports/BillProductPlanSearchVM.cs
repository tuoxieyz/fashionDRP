using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using ERPViewModelBasic;
using SysProcessViewModel;
using ManufacturingModel;
using SysProcessModel;
using ViewModelBasic;

namespace Manufacturing.ViewModel
{
    public class BillProductPlanSearchVM : BillPagedReportVM<BillProductPlanSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "生产品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
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
                        new FilterDescriptor("CreateDate", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("CreateDate", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        IEnumerable<ItemPropertyDefinition> _detailsPropertyDefinitions;
        public override IEnumerable<ItemPropertyDefinition> DetailsPropertyDefinitions
        {
            get
            {
                if (_detailsPropertyDefinitions == null)
                {
                    _detailsPropertyDefinitions = new List<ItemPropertyDefinition>() {
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) },
                        new ItemPropertyDefinition { DisplayName = "交货日期", PropertyName = "DeliveryDate", PropertyType = typeof(DateTime) }
                     };
                }
                return _detailsPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _detailsDescriptors;
        public override CompositeFilterDescriptorCollection DetailsDescriptors
        {
            get
            {
                if (_detailsDescriptors == null)
                {
                    _detailsDescriptors = new CompositeFilterDescriptorCollection() 
                    {
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("DeliveryDate", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _detailsDescriptors;
            }
        }

        protected override IEnumerable<BillProductPlanSearchEntity> SearchData()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var planContext = lp.GetDataContext<BillProductPlan>();
            var factoryContext = lp.GetDataContext<Factory>();
            var userContext = lp.GetDataContext<ViewUser>();

            var billData = from plan in planContext
                           from user in userContext
                           where plan.CreatorID == user.ID
                           select new BillProductPlanSearchEntity
                           {
                               ID = plan.ID,
                               BrandID = plan.BrandID,
                               Code = plan.Code,
                               CreateDate = plan.CreateTime.Date,
                               CreateTime = plan.CreateTime,
                               CreatorName = user.Name,
                               Remark = plan.Remark
                           };
            return SearchPlan(billData);
        }

        private List<BillProductPlanSearchEntity> SearchPlan(IQueryable<BillProductPlanSearchEntity> billData)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var detailsContext = lp.GetDataContext<BillProductPlanDetails>();
            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);
            billData = billData.Where(o => brandIDs.Contains(o.BrandID));
            if (FilterConditionHelper.IsConditionSetted(DetailsDescriptors, "StyleCode") || FilterConditionHelper.IsConditionSetted(DetailsDescriptors, "DeliveryDate"))
            {
                var productContext = lp.GetDataContext<ViewProduct>();
                var detailFilter = from detail in detailsContext
                                   from p in productContext
                                   where detail.ProductID == p.ProductID && brandIDs.Contains(p.BrandID)
                                   select new DetailsFiltetEntity { ProductID = p.ProductID, StyleCode = p.StyleCode, DeliveryDate = detail.DeliveryDate };
                detailFilter = (IQueryable<DetailsFiltetEntity>)detailFilter.Where(DetailsDescriptors);
                var pIDs = detailFilter.ToList().Select(p => p.ProductID);
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<BillProductPlanSearchEntity>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            //var datas = new QueryableCollectionView(filtedData.OrderBy(o => o.BillID).Skip(pageIndex * pageSize).Take(pageSize).ToList()); //filtedData.ToList();
            var plans = filtedData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();//(List<OrderSearchEntity>)datas.SourceCollection;
            var bIDs = plans.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity), QuaCancel = g.Sum(o => o.QuaCancel), QuaCompleted = g.Sum(o => o.QuaCompleted) }).ToList();
            plans.ForEach(d =>
            {
                d.BrandName = brands.Find(o => d.BrandID == o.ID).Name;
                var plan = sum.Find(o => o.BillID == d.ID);
                d.Quantity = plan.Quantity;
                d.QuaCancel = plan.QuaCancel;
                d.QuaCompleted = plan.QuaCompleted;
                var realOrderQuantity = d.Quantity - d.QuaCancel;
                d.StatusName = realOrderQuantity == d.QuaCompleted ? "已完成" : (d.QuaCompleted == 0 ? "未交货" : (realOrderQuantity > d.QuaCompleted ? "部分已交货" : "数据有误"));
            });
            return plans;
        }        
    }

    internal class DetailsFiltetEntity
    {
        public int ProductID { get; set; }
        public string StyleCode { get; set; }
        public DateTime DeliveryDate { get; set; }
    }
}
