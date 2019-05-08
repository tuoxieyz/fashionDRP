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
    public class SubordinateStocktakeAggregationVM : BillReportWithHorSizeVM<DistributionProductShow>
    {
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
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "盘点品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "库存更新状态", PropertyName = "Status", PropertyType = typeof(bool)}
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
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Status", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<DistributionProductShow> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = childOrganizations.Select(o => o.ID);
            var stocktakeContext = lp.Search<BillStocktake>(o => oids.Contains(o.OrganizationID) && !o.IsDeleted && brandIDs.Contains(o.BrandID));
            var detailsContext = lp.GetDataContext<BillStocktakeDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from st in stocktakeContext
                       from details in detailsContext
                       where st.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID
                       select new StocktakeEntityForAggregation
                       {
                           ID = st.ID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           Code = st.Code,
                           CreateTime = st.CreateTime.Date,
                           Status = st.Status,
                           StorageID = st.StorageID,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity,
                           OrganizationID = st.OrganizationID
                       };
            var filtedData = (IQueryable<StocktakeEntityForAggregation>)data.Where(FilterDescriptors);
            return ReportDataContext.AggregateBill(filtedData);
        }
    }
}
