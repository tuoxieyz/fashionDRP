using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using System.Collections.ObjectModel;
using ERPViewModelBasic;
using SysProcessModel;

namespace DistributionViewModel
{
    public class BillSubordinateOrderSearchVM : BillPagedReportVM<OrderSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "订货品牌", PropertyName = "BrandID", PropertyType = typeof(int)}
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

        /// <summary>
        /// 查询本级和下级订单
        /// </summary>
        protected override IEnumerable<OrderSearchEntity> SearchData()
        {
            int totalCount = 0;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var data = ReportDataContext.SearchBillOrder(FilterDescriptors, DetailsDescriptors, oids, PageIndex, PageSize, ref totalCount);
            TotalCount = totalCount;
            return data;
        }
    }
}
