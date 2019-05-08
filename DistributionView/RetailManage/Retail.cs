using DistributionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionView.RetailManage
{
    public class Retail : BaseRetail
    {
        public Retail()
            : base(new BillRetailVM(), false)
        {
        }
    }
}
