using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace DistributionModel
{
    /// <summary>
    /// 出库单明细
    /// </summary>
    public class BillStoreOutDetails : BillDetailBase
    {
        /// <summary>
        /// 实现隐式从基类到子类出库单明细的转换
        /// </summary>
        /// <remarks>当运行时类型非子类型时（可能为基类或其它派生类），.net默认的转换将抛出异常,因此重载一个转换逻辑</remarks>
        //咳咳，可惜这种方式编译器不允许，蛋疼
        //public static implicit operator BillStoreOutDetails(BillDetailBase bdBase)//隐式转换
        //{
        //    BillStoreOutDetails soDetails = new BillStoreOutDetails();
        //    soDetails.BillID = bdBase.BillID;
        //    soDetails.ProductID = bdBase.ProductID;
        //    soDetails.Quantity = bdBase.Quantity;
        //    return soDetails;
        //}
    }
}

