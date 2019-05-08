using DistributionModel;
using DomainLogicEncap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class ContractDiscountHelper
    {
        private List<OrganizationContractDiscount> _discountCache = new List<OrganizationContractDiscount>();

        /// <summary>
        /// 获取折扣
        /// </summary>
        public decimal GetDiscount(int brandID, int year, int quarter, int organizationID)
        {
            var byq = ProductLogic.GetBYQ(brandID, year, quarter);
            if (byq == null)
                throw new Exception("没有对应品牌年份季度信息");
            return GetDiscount(byq.ID, organizationID);
        }

        /// <summary>
        /// 获取折扣
        /// </summary>
        public decimal GetDiscount(int byqID, int organizationID)
        {
            var dc = _discountCache.Find(o => o.OrganizationID == organizationID && o.BYQID == byqID);
            if (dc == null)
            {
                dc = OrganizationLogic.GetOrganizationContractDiscount(byqID, organizationID);
                if (dc == null)//未设置折扣
                    dc = new OrganizationContractDiscount { OrganizationID = organizationID, BYQID = byqID, Discount = 100 };
                _discountCache.Add(dc);
            }
            return dc.Discount;
        }
    }
}
