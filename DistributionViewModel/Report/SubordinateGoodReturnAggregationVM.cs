using DistributionModel;
using ERPViewModelBasic;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class SubordinateGoodReturnAggregationVM : BillReportWithHorSizeVM<DistributionProductShow>
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
                         new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                new ItemPropertyDefinition { DisplayName = "退货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
                new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(int) },
                new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)}
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

        protected override IEnumerable<DistributionProductShow> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var goodReturnContext = lp.Search<BillGoodReturn>(o => oids.Contains(o.OrganizationID) && brandIDs.Contains(o.BrandID));
            var detailsContext = lp.GetDataContext<BillGoodReturnDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();

            var data = from goodreturn in goodReturnContext
                       from details in detailsContext
                       where goodreturn.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new MultiStatusBillEntityForAggregation
                       {
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = goodreturn.CreateTime.Date,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           Status = goodreturn.Status,
                           NameID = product.NameID,
                           OrganizationID = goodreturn.OrganizationID
                       };

            data = (IQueryable<MultiStatusBillEntityForAggregation>)data.Where(FilterDescriptors);
            return ReportDataContext.AggregateBill(data);
        }
    }
}
