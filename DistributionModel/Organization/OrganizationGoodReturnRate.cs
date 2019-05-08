using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace DistributionModel
{
    public class OrganizationGoodReturnRate : CreatedData, IDEntity
    {
        [ColumnAttribute(IsPrimaryKey = true, IsGenerated = true)]
        public int ID { get; set; }
        public int OrganizationID { get; set; }
        public int BrandID { get; set; }
        private decimal _goodReturnRate = 100;
        public decimal GoodReturnRate { get { return _goodReturnRate; } set { _goodReturnRate = value; } }
    }

    public class OrganizationGoodReturnRatePerQuarter : CreatedData, IDEntity
    {
        [ColumnAttribute(IsPrimaryKey = true, IsGenerated = true)]
        public int ID { get; set; }
        public int RateID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        private decimal _goodReturnRate = 100;
        public decimal GoodReturnRate { get { return _goodReturnRate; } set { _goodReturnRate = value; } }
    }
}
