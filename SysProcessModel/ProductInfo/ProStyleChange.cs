using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SysProcessModel
{
    [DataContract]
    public class ProStyleChange
    {
        [DataMember]
        public int StyleID { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int CreatorID { get; set; }
        [DataMember]
        public DateTime CreateTime { get; set; }
    }
}
