using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;

namespace DistributionViewModel
{
    public class FundAccountSearchEntity : OrganizationFundAccount
    {
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; }
        public string BrandName { get; set; }        
        public DateTime OccurDate { get; set; }
        public string RefrenceBillKind { get; set; }
        /// <summary>
        /// 发生日期时的余额
        /// </summary>
        public decimal BalanceAtThatTime { get; set; }
    }
}
