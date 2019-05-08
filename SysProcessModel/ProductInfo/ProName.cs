using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using Model.Extension;
namespace SysProcessModel
{
    //ProName
    public class ProName : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        private bool _flag = true;
        public bool Flag
        {
            get { return _flag; }
            set { _flag = value; }
        }
    }
}

