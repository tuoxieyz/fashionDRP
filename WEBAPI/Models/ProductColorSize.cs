using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBAPI
{
    public class ProductColorSize
    {
        public IEnumerable<string> ColorNames { get; set; }
        public IEnumerable<string> SizeNames { get; set; }
    }
}