using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using System.Collections.ObjectModel;
using DomainLogicEncap;
using ViewModelBasic;
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class RetailAggregationVM : BillReportWithHorSizeVM<RetailAggregationEntity>
    {
        protected override IEnumerable<string> PropertyNamesForSum
        {
            get
            {
                return new string[] { "Quantity", "CutMoney", "CostMoney" };
            }
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) },
                        new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "班次", PropertyName = "ShiftID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "导购", PropertyName = "GuideID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "VIP卡号", PropertyName = "VIPCode", PropertyType = typeof(string) }
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<RetailAggregationEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var retailContext = lp.Search<BillRetail>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var vipContext = lp.GetDataContext<VIPCard>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var data = from retail in retailContext
                       from details in detailsContext
                       where retail.ID == details.BillID
                       from product in productContext
                       join vip in vipContext on retail.VIPID equals vip.ID into vips
                       from v in vips.DefaultIfEmpty()//生成Left [outer] join左联接语句，就不会产生重复数据了 
                       where product.ProductID == details.ProductID && brandIDs.Contains(product.BrandID)
                       select new RetailEntityForAggregation
                       {
                           OrganizationID = retail.OrganizationID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           StorageID = retail.StorageID,
                           CreateTime = retail.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           DiscountMoney = details.Price * details.Quantity * details.Discount / 100,
                           CutMoney = details.CutMoney,
                           VIPCode = v.Code,
                           ShiftID = retail.ShiftID,
                           GuideID = retail.GuideID,
                           //Year = product.Year,
                           //Quarter = product.Quarter,
                           BYQID = product.BYQID
                       };
            data = (IQueryable<RetailEntityForAggregation>)data.Where(FilterDescriptors);
            var result = ReportDataContext.AggregateBillRetail(data);
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            result.ForEach(o =>
            {
                o.Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.BYQID, o.Price);
                o.CostMoney = o.DiscountMoney - o.CutMoney;
            });
            return result;
        }
    }
}
