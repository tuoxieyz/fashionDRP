using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;

namespace DistributionViewModel
{
    public class FundAccountTotalEntity : OrganizationFundAccount
    {
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; }
        public string BrandName { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }
    }
}
