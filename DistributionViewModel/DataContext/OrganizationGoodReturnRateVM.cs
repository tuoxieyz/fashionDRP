using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using SysProcessViewModel;
using Kernel;
using System.Transactions;

namespace DistributionViewModel
{
    public class OrganizationGoodReturnRateVM : PagedEditSynchronousVM<OrganizationGoodReturnRate>
    {
        #region 属性

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}, 
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
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public OrganizationGoodReturnRateVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = new List<OrganizationGoodReturnRateBO>();
        }

        protected override IEnumerable<OrganizationGoodReturnRate> SearchData()
        {            
            var oids = OrganizationListVM.CurrentOrganization.ChildrenOrganizations.Select(o=>o.ID).ToArray();
            var all = VMGlobal.DistributionQuery.LinqOP.Search<OrganizationGoodReturnRate>(o => oids.Contains(o.OrganizationID));
            var filteredData = (IQueryable<OrganizationGoodReturnRate>)all.Where(FilterDescriptors);
            TotalCount = filteredData.Count();
            var data = filteredData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            return data.Select(o => new OrganizationGoodReturnRateBO(o)).ToList();
        }

        private bool RateIsExist(OrganizationGoodReturnRate rate)
        {
            if (rate.ID == default(int))
            {
                return LinqOP.Any<OrganizationGoodReturnRate>(oc => oc.OrganizationID == rate.OrganizationID && oc.BrandID == rate.BrandID);
            }
            else
            {
                return LinqOP.Any<OrganizationGoodReturnRate>(oc => oc.ID != rate.ID && oc.OrganizationID == rate.OrganizationID && oc.BrandID == rate.BrandID);
            }
        }

        public override OPResult AddOrUpdate(OrganizationGoodReturnRate entity)
        {
            if (RateIsExist(entity))
            {
                return new OPResult { IsSucceed = false, Message = "该机构已经设置了该品牌的退货率." };
            }
            return base.AddOrUpdate(entity);
        }

        public override OPResult Delete(OrganizationGoodReturnRate entity)
        {
            if (entity.ID == default(int))
                return new OPResult { IsSucceed = true, Message = "删除成功!" };
            var bo = (OrganizationGoodReturnRateBO)entity;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Delete<OrganizationGoodReturnRatePerQuarter>(o => o.RateID == entity.ID);
                    LinqOP.Delete<OrganizationGoodReturnRate>(entity);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "删除成功!" };
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
                }
            }
        }

        private bool RateIsExist(OrganizationGoodReturnRatePerQuarter rate)
        {
            if (rate.ID == default(int))
            {
                return LinqOP.Any<OrganizationGoodReturnRatePerQuarter>(oc => oc.RateID == rate.RateID && oc.Year == rate.Year && oc.Quarter == rate.Quarter);
            }
            else
            {
                return LinqOP.Any<OrganizationGoodReturnRatePerQuarter>(oc => oc.ID != rate.ID && oc.RateID == rate.RateID && oc.Year == rate.Year && oc.Quarter == rate.Quarter);
            }
        }

        public OPResult AddOrUpdate(OrganizationGoodReturnRatePerQuarter entity)
        {
            if (RateIsExist(entity))
            {
                return new OPResult { IsSucceed = false, Message = "该机构已经设置了该品牌年份季度的退货率." };
            }
            var id = entity.ID;
            try
            {
                if (id == default(int))
                {
                    entity.ID = LinqOP.Add<OrganizationGoodReturnRatePerQuarter, int>(entity, o => o.ID);
                }
                else
                {
                    LinqOP.Update<OrganizationGoodReturnRatePerQuarter>(entity);
                }
            }
            catch (Exception e)
            {
                entity.ID = id;
                return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
            }

            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        public OPResult Delete(OrganizationGoodReturnRatePerQuarter entity)
        {
            if (entity.ID == default(int))
                return new OPResult { IsSucceed = true, Message = "删除成功!" };
            try
            {
                LinqOP.Delete<OrganizationGoodReturnRatePerQuarter>(entity);
                return new OPResult { IsSucceed = true, Message = "删除成功!" };
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
            }
        }
    }
}
