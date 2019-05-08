using DistributionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    public class BillGoodReturnRejectBO : BillBO<BillGoodReturn, BillGoodReturnDetails>
    {
        public OrganizationFundAccount FundAccount { get; set; }
    }
}
