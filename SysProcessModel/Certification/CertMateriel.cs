using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace SysProcessModel
{
    public class CertMateriel : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        private bool _enabled = true;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
            }
        }
    }
}
