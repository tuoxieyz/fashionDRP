﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel.Finance;
using Telerik.Windows.Data;
using DBAccess;
using DistributionModel;
using DomainLogicEncap;
using DistributionViewModel;
using System.ComponentModel;
using SysProcessModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Kernel;
using System.Transactions;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;
using ERPModelBO;

namespace DistributionViewModel
{
    public class VoucherDeductMoneyVM : PagedEditSynchronousVM<VoucherDeductMoney>
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

        public VoucherDeductMoneyVM(bool isAudit = false)
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            if (isAudit)//审核
            {
                FilterDescriptors.Add(new FilterDescriptor("Status", FilterOperator.IsEqualTo, false));
                Entities = this.SearchData();
            }
            else
            {
                ItemPropertyDefinitions.Add(new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(bool) });
                Entities = new List<VoucherDeductMoney>();
            }
        }

        protected override IEnumerable<VoucherDeductMoney> SearchData()
        {
            var oids = (OrganizationArray ?? OrganizationListVM.CurrentOrganization.ChildrenOrganizations).Select(o => o.ID);
            var data = (IQueryable<VoucherDeductMoney>)LinqOP.Search<VoucherDeductMoney>(o => oids.Contains(o.OrganizationID)).Where(FilterDescriptors);
            TotalCount = data.Count();
            return data.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }

        public override OPResult AddOrUpdate(VoucherDeductMoney entity)
        {
            if (entity.ID == default(int))
            {
                entity.Code = BillHelper.GenerateBillCode<VoucherDeductMoney>(entity);
            }
            return base.AddOrUpdate(entity);
        }

        public override OPResult Delete(VoucherDeductMoney entity)
        {
            if (entity.Status)
            {
                return new OPResult { IsSucceed = false, Message = "不能删除已审核单据." };
            }
            return base.Delete(entity);
        }

        public OPResult Audit(VoucherDeductMoney entity)
        {
            if (entity.ID == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "请先保存单据." };
            }
            if (entity.Status)
            {
                return new OPResult { IsSucceed = false, Message = "该扣款单已审核." };
            }
            entity.Status = true;
            entity.CheckerID = VMGlobal.CurrentUser.ID;
            entity.CheckTime = DateTime.Now;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Update<VoucherDeductMoney>(entity);
                    LinqOP.Add<OrganizationFundAccount>(
                        new OrganizationFundAccount
                        {
                            BrandID = entity.BrandID,
                            OrganizationID = entity.OrganizationID,
                            NeedIn = entity.DeductMoney,
                            AlreadyIn = 0.0M,
                            CreatorID = entity.CreatorID,
                            BillKind = (int)BillTypeEnum.VoucherDeductMoney,
                            Remark = "扣款单生成",
                            RefrenceBillCode = entity.Code
                        });
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "审核成功." };
                }
                catch (Exception e)
                {
                    entity.Status = false;
                    return new OPResult { IsSucceed = false, Message = "审核失败,失败原因:\n" + e.Message };
                }
            }
        }
    }
}
