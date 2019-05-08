using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace SysProcessModel
{
    public class CertGrade : IDNameEntity
    {
        public string Name
        {
            get;
            set;
        }

        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID
        {
            get;
            set;
        }

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
