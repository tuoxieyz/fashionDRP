//*********************************************
// 公司名称：
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-5-23 9:53:31
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using DistributionViewModel;
using System.Data;

namespace DistributionView
{
    public class TotalPriceFunction : AggregateFunction<DistributionProductShow, decimal>
    {
        public TotalPriceFunction()
        {
            this.AggregationExpression = products => products.Sum(p => p.Price * p.Quantity);
            this.ResultFormatString = "价格总计:{0:C2}";
        }
    }

    public class TotalDiscountPriceFunction : AggregateFunction<DistributionProductShow, decimal>
    {
        public TotalDiscountPriceFunction()
        {
            this.AggregationExpression = products => products.Sum(p => p.Price * p.Quantity * p.Discount * 0.01M);
            if (string.IsNullOrEmpty(Caption))
                Caption = "折扣价总计:";
            this.ResultFormatString = "{0:C2}";
        }
    }

    public class TotalPriceFunctionForDataTable : AggregateFunction<DataRow, decimal>
    {
        public TotalPriceFunctionForDataTable()
        {
            this.AggregationExpression = products => products.Sum(p => (decimal)p["Price"] * (int)p["Quantity"]);
            this.ResultFormatString = "价格总计:{0:C2}";
        }
    }
}
