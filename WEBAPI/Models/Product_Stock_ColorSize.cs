using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBAPI
{
    public class Product_Stock_ColorSize
    {
        /// <summary>
        /// 库存分布
        /// </summary>
        public IEnumerable<ProductStock> ShopStocks { get; set; }
        /// <summary>
        /// 颜色尺码（只包含有库存的）
        /// </summary>
        public ProductColorSize ColorSize { get; set; }
    }
}