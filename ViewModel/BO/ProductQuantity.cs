using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPViewModelBasic
{
    public interface IProductForAggregation
    {
        int ProductID { get; set; }
        int Quantity { get; set; }
    }

    public class ProductQuantity : IProductForAggregation
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}
