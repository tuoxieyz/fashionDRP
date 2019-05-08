using DistributionModel;
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
    public class StyleRetailRankingVM : CommonViewModel<RetailAggregationEntity>
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
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "色号", PropertyName = "ColorCode", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "尺码", PropertyName = "SizeID", PropertyType = typeof(int)}, 
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)}
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
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("NameID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        private IEnumerable<int> _organizationIDArray;
        public IEnumerable<int> OrganizationIDArray
        {
            get
            {
                if (_organizationIDArray == null || _organizationIDArray.Count() == 0)
                    return null;
                return _organizationIDArray;
            }
            set
            {
                _organizationIDArray = value;
            }
        }

        private DateTime _startDate = DateTime.Now.Date;
        public DateTime StartDate { get { return _startDate; }
            set {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged("StartDate");
                }
            }
        }

        private DateTime _endDate = DateTime.Now.Date;
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged("EndDate");
                }
            }
        }

        protected override IEnumerable<RetailAggregationEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationListVM.CurrentAndChildrenOrganizations.Select(o => o.ID).ToArray();
            var retailContext = lp.GetDataContext<BillRetail>();
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var orgContext = lp.Search<ViewOrganization>(o => oids.Contains(o.ID) && o.Flag);
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var data = from retail in retailContext
                       from org in orgContext
                       where retail.OrganizationID == org.ID
                       from details in detailsContext
                       where retail.ID == details.BillID
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
                           AreaID = org.AreaID,
                           ProvienceID = org.ProvienceID,
                           OrganizationTypeID = org.TypeId,
                           CostMoney = details.Price * details.Quantity * details.Discount / 100,
                           CutMoney = details.CutMoney
                       };
            data = (IQueryable<RetailEntityForAggregation>)data.Where(filters);
            var result = AggregateBillRetail(data);
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            result.ForEach(o => o.Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, o.ProductID));
            return result;
        }
    }
}
