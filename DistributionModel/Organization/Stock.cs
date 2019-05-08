//*********************************************
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-5-25 11:59:20
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    /// <summary>
    /// 库存
    /// </summary>
    public class Stock
    {
        [ColumnAttribute(IsPrimaryKey = true)]
        public int StorageID { get; set; }
        [ColumnAttribute(IsPrimaryKey = true)]
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}
