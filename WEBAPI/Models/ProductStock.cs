using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBAPI
{
    public class ProductStock
    {
        public int ShopID { get; set; }
        public string StyleCode { get; set; }
        public List<ColorQuantity> ColorQuas { get; set; }
    }
}