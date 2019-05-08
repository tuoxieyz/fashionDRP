using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using System.ComponentModel;
using Model.Extension;

namespace SysProcessModel
{
    public class ProColor : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string RGBCode { get; set; }
    }
}
