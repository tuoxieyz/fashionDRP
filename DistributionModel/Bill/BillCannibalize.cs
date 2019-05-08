using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace DistributionModel
{
    /// <summary>
    /// 调拨单
    /// </summary>
    public class BillCannibalize : BillWithBrand, IStorageID
    {
        public int StorageID { get; set; }
        public int ToOrganizationID { get; set; }
        public bool Status { get; set; }
    }
}

