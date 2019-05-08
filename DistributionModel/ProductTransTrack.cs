using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;
using System.Runtime.Serialization;

namespace DistributionModel
{
    [DataContract]
    public class BillSnapshot:IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public int BillID { get; set; }
        [DataMember]
        public string BillCode { get; set; }
        [DataMember]
        public string BillTypeName { get; set; }
        [DataMember]
        public string OrganizationName { get; set; }
        [DataMember]
        public string CreatorName { get; set; }
        [DataMember]
        public DateTime CreateTime { get; set; }
        [DataMember]
        public string Remark { get; set; }
    }

    [DataContract]
    public class BillSnapshotDetailsWithUniqueCode
    {
        [DataMember]
        public int SnapshotID { get; set; }
        [DataMember]
        public int ProductID { get; set; }
        [DataMember]
        public string UniqueCode { get; set; }
    }
}
