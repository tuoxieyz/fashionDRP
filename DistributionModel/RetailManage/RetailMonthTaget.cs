using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    public class RetailMonthTaget : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int OrganizationID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        private int _saleTarget;
        public int SaleTaget
        {
            get { return _saleTarget; }
            set
            {
                _saleTarget = value;
            }
        }
    }
}
