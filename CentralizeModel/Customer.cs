using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace CentralizeModel
{
    public class Customer
    {
        private int _iD;
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID
        {
            get
            {
                return this._iD;
            }
            set
            {
                this._iD = value;
            }
        }

        public string IdentificationKey
        {
            get;
            set;
        }

        private string _name;
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        private string _linkman;
        public string Linkman
        {
            get
            {
                return this._linkman;
            }
            set
            {
                this._linkman = value;
            }
        }

        private string _phone;
        public string Phone
        {
            get
            {
                return this._phone;
            }
            set
            {
                this._phone = value;
            }
        }

        private string _address;
        public string Address
        {
            get
            {
                return this._address;
            }
            set
            {
                this._address = value;
            }
        }

        public int UserPointLimit { get; set; }

        public string ApiUrls { get; set; }

    }
}
