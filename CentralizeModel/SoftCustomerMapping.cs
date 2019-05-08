using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace CentralizeModel
{
    public class SoftCustomerMapping
    {
        [ColumnAttribute(IsPrimaryKey = true, IsGenerated = true)]
        public int ID { get; set; }

        private int _softID;
        public int SoftID
        {
            get
            {
                return this._softID;
            }
            set
            {
                this._softID = value;
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
