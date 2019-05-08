using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBAPI
{
    public class ColorQuantity
    {
        public string ColorName { get; set; }
        public List<SizeQuantity> SizeQuas { get; set; }
    }
}