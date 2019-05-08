using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    //VipUpTactic
    public class VIPUpTactic : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        public int FormerKindID { get; set; }
        public int AfterKindID { get; set; }
        public int OnceConsume { get; set; }
        public int DateSpan { get; set; }
        public int SpanConsume { get; set; }
        public int BrandID { get; set; }
        public int CutPoint { get; set; }
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                }
            }
        }
    }
}

