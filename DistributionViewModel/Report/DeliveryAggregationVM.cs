using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using DistributionModel;
using SysProcessModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using ViewModelBasic;
using SysProcessViewModel;
using Telerik.Windows.Controls;
using System.Data;

namespace DistributionViewModel
{
    public class DeliveryAggregationVM : ViewModelBase
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public DataTable Entities { get; set; }

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
                        new ItemPropertyDefinition { DisplayName = "发货品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "出货仓库", PropertyName = "StorageID", PropertyType = typeof(int) },
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
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        public System.Windows.Input.ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(param =>
                {
                    Entities = this.SearchData();
                    OnPropertyChanged("Entities");
                });
            }
        }

        /// <summary>
        /// 发货单汇总
        /// </summary>
        private DataTable SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var deliveryContext = lp.Search<BillDelivery>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            var deliveryDetailsContext = lp.GetDataContext<BillDeliveryDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            //FilterBillWithBrand(deliveryContext, filters, brandIDs);
            //假如未设置机构条件，则默认下级所有机构
            //if (!FilterConditionHelper.IsConditionSetted(filters, "ToOrganizationID"))
            //{
            //    var oids = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Select(o => o.ID).ToArray();
            //    orgContext = orgContext.Where(o => oids.Contains(o.ID));
            //}
            var data = from delivery in deliveryContext
                       from deDetails in deliveryDetailsContext
                       where delivery.ID == deDetails.BillID && oids.Contains(delivery.ToOrganizationID) && brandIDs.Contains(delivery.BrandID)
                       from product in productContext
                       where product.ProductID == deDetails.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new BillDeliveryForAggregation
                       {
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           CreateTime = delivery.CreateTime.Date,
                           //ProductCode = product.ProductCode,
                           //BrandCode = product.BrandCode,
                           StyleCode = product.StyleCode,
                           //ColorCode = product.ColorCode,
                           //SizeName = product.SizeName,
                           //Price = product.Price,
                           Quantity = deDetails.Quantity,
                           Status = delivery.Status,
                           NameID = product.NameID,
                           Year = product.Year,
                           Quarter = product.Quarter
                       };

            data = (IQueryable<BillDeliveryForAggregation>)data.Where(FilterDescriptors);
            return new BillReportHelper().TransferSizeToHorizontal<DistributionProductShow>(ReportDataContext.AggregateBill(data));
        }

        private class BillDeliveryForAggregation : BillEntityForAggregation
        {
            public int Status { get; set; }
        }
    }
}
