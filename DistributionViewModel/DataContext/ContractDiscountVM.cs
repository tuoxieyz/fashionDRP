using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Kernel;
using DomainLogicEncap;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;
using SysProcessModel;

namespace DistributionViewModel
{
    public class ContractDiscountVM : PagedEditSynchronousVM<OrganizationContractDiscount>
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
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)}
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

        public ContractDiscountVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = new List<OrganizationContractDiscountBO>();
        }

        protected override IEnumerable<OrganizationContractDiscount> SearchData()
        {
            var opfs = LinqOP.GetDataContext<OrganizationContractDiscount>();
            var byqs = VMGlobal.DistributionQuery.QueryProvider.GetTable<ProBYQ>("SysProcess.dbo.ProBYQ");
            var childOrganizationIDs = VMGlobal.ChildOrganizations.Select(o => o.ID);
            var data = from opf in opfs
                       from byq in byqs
                       where opf.BYQID == byq.ID && childOrganizationIDs.Contains(opf.OrganizationID)
                       select new OrganizationContractDiscountBO
                       {
                           BrandID = byq.BrandID,
                           Year = byq.Year,
                           Quarter = byq.Quarter,
                           BYQID = byq.ID,
                           ID = opf.ID,
                           CreateTime = opf.CreateTime,
                           CreatorID = opf.CreatorID,
                           Discount = opf.Discount,
                           OrganizationID = opf.OrganizationID
                       };
            var filteredData = (IQueryable<OrganizationContractDiscountBO>)data.Where(FilterDescriptors);
            TotalCount = filteredData.Count();
            var result = filteredData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            //result.ForEach(o =>
            //{
            //    var co = VMGlobal.ChildOrganizations.Find(c => c.ID == o.OrganizationID);
            //    if (co != null)
            //    {
            //        o.OrganizationCode = co.Code;
            //        o.OrganizationName = co.Name;
            //    }
            //});
            return result;
        }

        public override OPResult AddOrUpdate(OrganizationContractDiscount entity)
        {
            OrganizationContractDiscountBO contractdiscount = (OrganizationContractDiscountBO)entity;
            var byq = ProductLogic.GetBYQ(contractdiscount.BrandID, contractdiscount.Year, contractdiscount.Quarter);
            if (byq == null)
            {
                return new OPResult { IsSucceed = false, Message = "未找到相应的品牌年份季度信息" };
            }
            contractdiscount.BYQID = byq.ID;
            bool isAdd = contractdiscount.ID == default(int);
            if (isAdd)
            {
                if (LinqOP.Any<OrganizationContractDiscount>(o => o.OrganizationID == contractdiscount.OrganizationID && o.BYQID == contractdiscount.BYQID))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构指定了对应款式的合同折扣" };
                }
            }
            else
            {
                if (LinqOP.Any<OrganizationContractDiscount>(o => o.OrganizationID == contractdiscount.OrganizationID && o.ID != contractdiscount.ID && o.BYQID == contractdiscount.BYQID))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构指定了对应款式的合同折扣" };
                }
            }
            return base.AddOrUpdate(entity);
        }
    }
}
