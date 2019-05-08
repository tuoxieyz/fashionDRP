using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using Model.Extension;
namespace DistributionModel
{
    //BillOrderDetails
    public class BillOrderDetails : BillDetailBase
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int QuaCancel { get; set; }
        /// <summary>
        /// 发货数量or配货数量(假如强制走配货流程的话)
        /// </summary>
        public int QuaDelivered { get; set; }
        public int Status { get; set; }
        [ColumnAttribute(IsGenerated = true)]
        public bool IsDeleted { get; set; }
    }
}

