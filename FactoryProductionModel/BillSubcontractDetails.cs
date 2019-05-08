using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace ManufacturingModel
{
    public class BillSubcontractDetails:BillDetailBase
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int QuaCancel { get; set; }
        public int QuaCompleted { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public DateTime DeliveryDate { get; set; }
        public int Status { get; set; }
        [ColumnAttribute(IsGenerated = true)]
        public bool IsDeleted { get; set; }
    }
}
