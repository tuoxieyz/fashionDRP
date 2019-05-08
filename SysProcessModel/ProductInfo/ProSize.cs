using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace SysProcessModel
{
    //ProSize
    public class ProSize : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        private bool _flag = true;
        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }

    }
}

