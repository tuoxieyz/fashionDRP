using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Data;
using Kernel;
using System.Transactions;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel.Retail
{
    public class RetailTacticVM : PagedEditSynchronousVM<RetailTactic>
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
                        new ItemPropertyDefinition { DisplayName = "开始日期", PropertyName = "BeginDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "结束日期", PropertyName = "EndDate", PropertyType = typeof(DateTime)},                
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("BeginDate", FilterOperator.IsGreaterThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("EndDate", FilterOperator.IsLessThanOrEqualTo, FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public RetailTacticVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<RetailTactic> SearchData()
        {
            var tactics = LinqOP.GetDataContext<RetailTactic>();
            //if (!FilterConditionHelper.IsConditionSetted(condition, "OrganizationID"))
            //{
            var ds = VMGlobal.SysProcessQuery.DB.ExecuteDataSet("GetOrganizationHierarchy", VMGlobal.CurrentUser.OrganizationID);
            var table = ds.Tables[0];
            if (table.Rows.Count > 0)
            {
                IEnumerable<int> os = table.ToList<OrganizationHierarchy>().Select(o => o.OrganizationID);
                tactics = tactics.Where(o => os.Contains(o.OrganizationID));
            }
            //}
            //if (!FilterConditionHelper.IsConditionSetted(condition, "BrandID"))
            //{
            IEnumerable<int> bs = VMGlobal.PoweredBrands.Select(o => o.ID);
            tactics = tactics.Where(o => bs.Contains(o.BrandID));
            //}
            var filteredData = (IQueryable<RetailTactic>)tactics.Where(FilterDescriptors);
            TotalCount = filteredData.Count();
            return filteredData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).Select(o=>new RetailTacticBO(o)).ToList();
        }

        public override OPResult AddOrUpdate(RetailTactic tactic)
        {
            if ((tactic.CostMoney == null || tactic.CutMoney == null) && tactic.Discount == null)
            {
                return new OPResult { IsSucceed = false, Message = "策略未设置或设置未符合规则" };
            }
            return base.AddOrUpdate(tactic);
        }

        public override OPResult Delete(RetailTactic tactic)
        {
            if (tactic.OrganizationID != VMGlobal.CurrentUser.OrganizationID)
            {
                return new OPResult { IsSucceed = false, Message = "只能删除本机构创建的零售策略" };
            }
            return base.Delete(tactic);
        }

        public OPResult SetStylesForTactic(int tacticID, IEnumerable<RetailTacticProStyleMapping> mapping)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Delete<RetailTacticProStyleMapping>(o => o.TacticID == tacticID);
                    LinqOP.Add<RetailTacticProStyleMapping>(mapping);
                    scope.Complete();
                    return new OPResult { IsSucceed = true };
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "设置失败,失败原因:\n" + e.Message };
                }
            }
        }
    }
}
