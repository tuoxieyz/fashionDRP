using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace CentralizeModel
{
    public class SoftVersionCustomerMapping
    {
        [ColumnAttribute(IsPrimaryKey = true, IsGenerated = true)]
        public int ID { get; set; }

        private int _softVersionID;
        public int SoftVersionID
        {
            get
            {
                return this._softVersionID;
            }
            set
            {
                this._softVersionID = value;
            }
        }

        private int _customerID;
        public int CustomerID
        {
            get
            {
                return this._customerID;
            }
            set
            {
                this._customerID = value;
            }
        }
    }
}
