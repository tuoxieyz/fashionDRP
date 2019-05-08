using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace SysProcessModel
{
    /// <summary>
    /// 成品款式所属时间和品牌
    /// </summary>
    public class ProBYQ : IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int BrandID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
    }
}
