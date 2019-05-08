using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace ManufacturingModel
{
    /// <summary>
    /// 生产计划单
    /// </summary>
    public class BillProductPlan : BillBase, IDCodeEntity
    {
        public int BrandID { get; set; }
        public int Status { get; set; }
    }
}
