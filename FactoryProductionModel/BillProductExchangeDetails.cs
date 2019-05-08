using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;
using System.Runtime.Serialization;

namespace ManufacturingModel
{
    public class BillProductExchangeDetails : BillDetailBase
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
    }
}
