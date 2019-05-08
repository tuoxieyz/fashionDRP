using DistributionModel.Finance;
using Kernel;
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
    public class ReceiveMoneyUnfreezeVM : PagedEditSynchronousVM<VoucherReceiveMoney>
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
        public override CompositeFilterDescriptorCollection FilterDescriptors
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

        public ReceiveMoneyUnfreezeVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<VoucherReceiveMoney> SearchData()
        {
            var oids = (OrganizationArray ?? OrganizationListVM.CurrentOrganization.ChildrenOrganizations).Select(o => o.ID);
            var data = (IQueryable<VoucherReceiveMoney>)LinqOP.Search<VoucherReceiveMoney>(o => o.IsMoneyFrozen && o.Status && oids.Contains(o.OrganizationID)).Where(FilterDescriptors);
            TotalCount = data.Count();
            return data.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }

        public OPResult Unfreeze(VoucherReceiveMoney entity)
        {
            if (!entity.IsMoneyFrozen)
            {
                return new OPResult { IsSucceed = false, Message = "该收款单资金已解冻." };
            }
            entity.IsMoneyFrozen = false;
            try
            {
                LinqOP.Update<VoucherReceiveMoney>(entity);
                return new OPResult { IsSucceed = true, Message = "资金解冻成功." };
            }
            catch (Exception e)
            {
                entity.Status = false;
                return new OPResult { IsSucceed = false, Message = "资金解冻失败,失败原因:\n" + e.Message };
            }
        }
    }
}
