using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using Model.Extension;
namespace DistributionModel
{
    /// <summary>
    /// 盘点单
    /// </summary>
    public class BillStocktake : BillWithBrand, IsDeletedEntity, IStorageID
    {
        public int StorageID { get; set; }

        /// <summary>
        /// 盘点单状态:是否已更新库存
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}

