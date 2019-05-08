using DistributionModel;
using ERPModelBO;
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
    public class OrganizationFundAccountSearchVM : PagedReportVM<FundAccountSearchEntity>
    {
        #region 属性

        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        List<ItemPropertyDefinition> _itemPropertyDefinitions;
        public List<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "发生日期", PropertyName = "OccurDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("OccurDate", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("OccurDate", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        protected override IEnumerable<FundAccountSearchEntity> SearchData()
        {
            var oids = OrganizationArray.Select(o => o.ID);
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var fundAccountContext = lp.Search<OrganizationFundAccount>(o => oids.Contains(o.OrganizationID));
            var brands = VMGlobal.PoweredBrands;
            var data = from fa in fundAccountContext
                       select new FundAccountSearchEntity
                       {
                           OrganizationID = fa.OrganizationID,
                           BrandID = fa.BrandID,
                           AlreadyIn = fa.AlreadyIn,
                           NeedIn = fa.NeedIn,
                           OccurDate = fa.CreateTime.Date,
                           CreateTime = fa.CreateTime,
                           RefrenceBillCode = fa.RefrenceBillCode,
                           RefrenceBillKind = Enum.GetName(typeof(BillTypeEnum), fa.BillKind),
                           Remark = fa.Remark,
                           BalanceAtThatTime = fundAccountContext.Where(o => o.OrganizationID == fa.OrganizationID && o.BrandID == fa.BrandID && o.CreateTime <= fa.CreateTime).Sum(o => o.AlreadyIn - o.NeedIn)
                       };
            var filtedData = (IQueryable<FundAccountSearchEntity>)data.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            var result = filtedData.OrderByDescending(o => o.CreateTime).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            result.ForEach(d =>
            {
                d.BrandName = brands.FirstOrDefault(b => b.ID == d.BrandID).Name;
                var typeField = typeof(BillTypeEnum).GetField(d.RefrenceBillKind);
                var displayNames = typeField.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
                d.RefrenceBillKind = ((EnumDescriptionAttribute)displayNames[0]).Description;
                d.OrganizationCode = OrganizationArray.First(o => o.ID == d.OrganizationID).Code;
                d.OrganizationName = OrganizationArray.First(o => o.ID == d.OrganizationID).Name;
            });
            return result;
        }
    }
}
