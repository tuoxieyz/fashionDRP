using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace DistributionModel
{
    public class VIPBirthdayTactic : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }

        private decimal _discount = 100M;
        public decimal Discount { get { return _discount; } set { _discount = value; } }
        /// <summary>
        /// 是否折上折
        /// </summary>
        //public bool IsMultDiscount { get; set; }

        private int _pointTimes = 1;
        /// <summary>
        /// 积分倍数
        /// </summary>
        public int PointTimes { get { return _pointTimes; } set { _pointTimes = value; } }
        public int? QuantityLimit { get; set; }
        public int? MoneyLimit { get; set; }
        public int OrganizationID { get; set; }
    }
}
