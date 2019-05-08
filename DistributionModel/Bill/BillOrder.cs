using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DBLinqProvider.Data.Mapping;
using Model.Extension;
namespace DistributionModel
{
    /// <summary>
    /// 订单
    /// </summary>
    public class BillOrder : BillWithBrand, IsDeletedEntity
    {
        public int Status { get; set; }

        [ColumnAttribute(IsGenerated = true)]
        public bool IsDeleted { get; set; }
    }
}

