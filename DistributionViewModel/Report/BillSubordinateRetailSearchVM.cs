using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DomainLogicEncap;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class BillSubordinateRetailSearchVM : BillPagedReportVM<RetailSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "VIP卡号", PropertyName = "VIPCode", PropertyType = typeof(string) }
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

        protected override IEnumerable<RetailSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var retailContext = lp.Search<BillRetail>(o => oids.Contains(o.OrganizationID));
            var userContext = lp.GetDataContext<ViewUser>();
            var vipContext = lp.GetDataContext<VIPCard>();

            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);

            var billData = from d in retailContext
                           from user in userContext
                           where d.CreatorID == user.ID
                           //from vip in vipContext
                           //where (d.VIPID == vip.ID || d.VIPID == null) //可能会产生重复数据
                           join vip in vipContext on d.VIPID equals vip.ID into vips
                           from v in vips.DefaultIfEmpty()//生成Left [outer] join左联接语句，就不会产生重复数据了
                           select new RetailSearchEntity
                           {
                               OrganizationID = d.OrganizationID,//跟查询条件相关的属性需要显式声明[赋值]，即使父类里已经定义，否则在生成SQL语句的过程中会报错
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               CreateDate = d.CreateTime.Date,
                               CreateTime = d.CreateTime,
                               CreatorName = user.Name,
                               VIPID = d.VIPID,
                               //VIPCode = (d.VIPID == null ? "" : vipContext.First(v => v.ID == d.VIPID).Code),//这样是可以滴
                               VIPCode = v.Code,
                               VIPName = v.CustomerName,
                               Quantity = d.Quantity,
                               CostMoney = d.CostMoney,
                               ReceiveMoney = d.ReceiveMoney,
                               TicketMoney = d.TicketMoney,
                               PredepositPay = d.PredepositPay
                           };
            var detailsContext = lp.GetDataContext<BillRetailDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<RetailSearchEntity>)billData.Where(FilterDescriptors);
            filtedData = filtedData.Distinct();
            TotalCount = filtedData.Count();
            if (TotalCount == 0)
                return null;
            var retails = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            retails.ForEach(r =>
            {
                r.OrganizationName = OrganizationArray.First(o => o.ID == r.OrganizationID).Name;
            });
            return retails;
        }
    }
}
