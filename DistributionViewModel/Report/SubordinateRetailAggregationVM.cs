using DistributionModel;
using DomainLogicEncap;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class SubordinateRetailAggregationVM : CommonViewModel<RetailAggregationEntity>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public virtual IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
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
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public virtual CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date, false),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date, false),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<RetailAggregationEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var retailContext = lp.GetDataContext<BillRetail>();
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            //var orgContext = lp.Search<ViewOrganization>(o => oids.Contains(o.ID) && o.Flag);
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var data = from retail in retailContext
                       //from org in orgContext
                       //where retail.OrganizationID == org.ID
                       from details in detailsContext
                       where retail.ID == details.BillID && oids.Contains(retail.OrganizationID)
                       from product in productContext
                       where product.ProductID == details.ProductID && brandIDs.Contains(product.BrandID)
                       select new RetailEntityForAggregation
                       {
                           OrganizationID = retail.OrganizationID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = retail.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           //Year = product.Year,
                           //Quarter = product.Quarter,
                           BYQID = product.BYQID,
                           DiscountMoney = details.Price * details.Quantity * details.Discount / 100,//这里的CostMoney和Retail表里的CostMoney不一样，相差下面的CutMoney
                           CutMoney = details.CutMoney,
                           Year = product.Year,
                           Quarter = product.Quarter
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
