using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace SysProcessModel
{
    public class ProductUniqueCodeMapping
    {
        public int ProductID { get; set; }
        [ColumnAttribute(IsPrimaryKey = true)]
        public string UniqueCode { get; set; }
    }

    public class ViewProductUniqueCodeMapping : ProductUniqueCodeMapping
    { }
}
