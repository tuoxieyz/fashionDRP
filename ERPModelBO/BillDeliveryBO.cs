using DistributionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    public class BillDeliveryBO : BillBO<BillDelivery, BillDeliveryDetails>
    {
        public OrganizationFundAccount FundAccount { get; set; }
    }
}
