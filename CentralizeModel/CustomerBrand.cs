using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralizeModel
{
    public class CustomerBrand
    {
        public int ID { get; set; }

        public int CustomerID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 别名
        /// <remarks>简称或标识key</remarks>
        /// </summary>
        public string AliasName { get; set; }
    }
}
