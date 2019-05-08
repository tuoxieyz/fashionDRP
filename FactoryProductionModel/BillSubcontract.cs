using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace ManufacturingModel
{
    /// <summary>
    /// 外发生产单
    /// </summary>
    public class BillSubcontract : BillBase, IDCodeEntity
    {
        public int BrandID { get; set; }
        /// <summary>
        /// 外发工厂
        /// </summary>
        public int OuterFactoryID { get; set; }
        public int Status { get; set; }
        [ColumnAttribute(IsGenerated = true)]
        public virtual bool IsDeleted { get; set; }
    }
}
