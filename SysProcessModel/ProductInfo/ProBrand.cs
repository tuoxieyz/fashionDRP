using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using DBLinqProvider.Data.Mapping;
using System.ComponentModel;
using Model.Extension;

namespace SysProcessModel
{
    public class ProBrand : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        private bool _flag = true;
        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

        public string Description { get; set; }
    }
}

