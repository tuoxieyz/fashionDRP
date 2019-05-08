using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using DistributionModel;
using Model.Extension;

namespace DistributionModel
{
    public class BillWithBrand : BillBase
    {
        public int BrandID { get; set; }
    }
}
