using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using DistributionModel;

namespace ERPViewModelBasic
{
    /// <summary>
    /// 用于汇总的单据实体
    /// </summary>
    public class BillEntityForAggregation : IProductForAggregation
    {
        public int ProductID { get; set; }
        public int BYQID { get; set; }
        public int BrandID { get; set; }
        public int Quantity { get; set; }
        public int OrganizationID { get; set; }
        public DateTime CreateTime { get; set; }
        public int StorageID { get; set; }
        //public string ProductCode { get; set; }
        //public string BrandCode { get; set; }
        public string StyleCode { get; set; }
        //public string ColorCode { get; set; }
        //public string SizeName { get; set; }
        //public decimal Price { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int NameID { get; set; }
    }

    /// <summary>
    /// 有状态属性的单据汇总实体
    /// <remarks>状态属性标示是否已入库</remarks>
    /// </summary>
    public class StatusBillEntityForAggregation : BillEntityForAggregation
    {
        /// <summary>
        /// 是否已入库
        /// <remarks>true:已入库 false:在途中</remarks>
        /// </summary>
        public bool Status { get; set; }
    }

    public class MultiStatusBillEntityForAggregation : BillEntityForAggregation
    {
        public int Status { get; set; }
    }
}
