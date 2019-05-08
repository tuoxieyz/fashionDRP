using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace SysProcessModel
{
    //ProUnit
    public class ProUnit : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        private bool _flag = true;
        public bool Flag {
            get { return _flag; }
            set { _flag = value; }
        }
    }
}

