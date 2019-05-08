using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    public class VIPCardKindMapping
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int CardID { get; set; }
        public int KindID { get; set; }
    }
}
