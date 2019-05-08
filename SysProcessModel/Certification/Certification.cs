using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace SysProcessModel
{
    public class Certification : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }

        public virtual int StyleID { get; set; }
        public string Composition { get; set; }
        public int Grade { get; set; }
        public int CarriedStandard { get; set; }
        public int SafetyTechnique { get; set; }
        public string GBCode { get; set; }
        public virtual decimal Price { get; set; }
    }
}
