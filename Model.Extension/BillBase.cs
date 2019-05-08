using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using System.Runtime.Serialization;

namespace Model.Extension
{
    public class BillBase : CreatedData, IDCodeEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public int OrganizationID { get; set; }
        public string Remark { get; set; }
    }
}
