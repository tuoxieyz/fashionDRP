using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    public class VIPKind : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        public int BrandID { get; set; }
        //private bool _isEnabled = true;
        //public bool IsEnabled
        //{
        //    get { return _isEnabled; }
        //    set
        //    {
        //        if (_isEnabled != value)
        //        {
        //            _isEnabled = value;
        //        }
        //    }
        //}

        public string Description { get; set; }
        public int Discount { get; set; }
    }
}

