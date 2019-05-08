using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace DistributionModel
{
    public class Storage : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        public int OrganizationID { get; set; }
        private bool _flag = true;
        public bool Flag { get { return _flag; } set { _flag = value; } }
    }
}
