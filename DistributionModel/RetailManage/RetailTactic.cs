using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    public class RetailTactic : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        public int OrganizationID { get; set; }
        public int BrandID { get; set; }
        /// <summary>
        /// 策略类型1:满减2:折扣3:混合
        /// <remarks>由于零售单特性，若一个款同时对应多个类型的策略，只能先折扣再满减</remarks>
        /// </summary>
        public int Kind { get; set; }

        private DateTime _beginDate = DateTime.Now.Date;
        public DateTime BeginDate
        {
            get { return _beginDate; }
            set { _beginDate = value; }
        }
        public DateTime? EndDate { get; set; }
        public int? CostMoney { get; set; }
        public int? CutMoney { get; set; }
        public decimal? Discount { get; set; }
        public bool CanVIPApply { get; set; }
    }
}
