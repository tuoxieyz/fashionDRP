using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.Runtime.Serialization;

namespace ManufacturingModel
{
    public class BillProductExchange : BillBase, IDCodeEntity
    {
        public int BrandID { get; set; }
        public virtual int Status { get; set; }
        public virtual bool IsDeleted { get; set; }
        public int OuterFactoryID { get; set; }
    }
}
