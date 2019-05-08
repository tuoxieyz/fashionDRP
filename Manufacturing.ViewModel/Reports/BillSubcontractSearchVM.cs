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
using DistributionModel;
using SysProcessViewModel;
using ViewModelBasic;

namespace Manufacturing.ViewModel
{
    public class BillSubcontractSearchVM : BillPagedReportVM<BillSubcontractSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
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

        protected override IEnumerable<BillSubcontractSearchEntity> SearchData()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var subcontractContext = lp.GetDataContext<BillSubcontract>();
            var factoryContext = lp.GetDataContext<Factory>();
            var userContext = lp.GetDataContext<ViewUser>();

            var billData = from subcontract in subcontractContext
                           from user in userContext
                           where subcontract.CreatorID == user.ID
                           from factory in factoryContext
                           where subcontract.OuterFactoryID == factory.ID
                           select new BillSubcontractSearchEntity
                           {
                               ID = subcontract.ID,
                               BrandID = subcontract.BrandID,
                               OuterFactoryID = factory.ID,
                               OuterFactoryName = factory.Name,
                               Code = subcontract.Code,
                               CreateDate = subcontract.CreateTime.Date,
                               CreateTime = subcontract.CreateTime,
                               CreatorName = user.Name,
                               Remark = subcontract.Remark,
                               IsDeleted = subcontract.IsDeleted
                           };
            return SearchSubcontract(billData);
        }

        private List<BillSubcontractSearchEntity> SearchSubcontract(IQueryable<BillSubcontractSearchEntity> billData)
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var detailsContext = lp.GetDataContext<BillSubcontractDetails>();
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
            var filtedData = (IQueryable<BillSubcontractSearchEntity>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            //var datas = new QueryableCollectionView(filtedData.OrderBy(o => o.BillID).Skip(pageIndex * pageSize).Take(pageSize).ToList()); //filtedData.ToList();
            var subcontracts = filtedData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();//(List<OrderSearchEntity>)datas.SourceCollection;
            var bIDs = subcontracts.Select(o => (int)o.ID);
            var sum = detailsContext.Where(o => bIDs.Contains(o.BillID) && o.IsDeleted == false).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, Quantity = g.Sum(o => o.Quantity), QuaCancel = g.Sum(o => o.QuaCancel), QuaCompleted = g.Sum(o => o.QuaCompleted) }).ToList();
            subcontracts.ForEach(d =>
            {
                d.BrandName = brands.Find(o => d.BrandID == o.ID).Name;
                var subcontract = sum.Find(o => o.BillID == d.ID);
                d.Quantity = subcontract.Quantity;
                d.QuaCancel = subcontract.QuaCancel;
                d.QuaCompleted = subcontract.QuaCompleted;
                var realOrderQuantity = d.Quantity - d.QuaCancel;
                d.StatusName = realOrderQuantity == d.QuaCompleted ? "已完成" : (d.QuaCompleted == 0 ? "未交货" : (realOrderQuantity > d.QuaCompleted ? "部分已交货" : "数据有误"));
            });
            return subcontracts;
        }
    }
}
