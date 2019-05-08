using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using Model.Extension;
namespace SysProcessModel
{
    /// <summary>
    /// 成品资料信息
    /// </summary>
    public class Product : CreatedData
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public int StyleID { get; set; }
        public int ColorID { get; set; }
        public int SizeID { get; set; }

        private bool _flag = true;
        public bool Flag { get { return _flag; } set { _flag = value; } }
    }
}

