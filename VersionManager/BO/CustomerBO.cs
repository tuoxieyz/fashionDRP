using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CentralizeModel;
using DBAccess;
using Telerik.Windows.Controls;
using System.Collections.ObjectModel;

namespace VersionManager.BO
{
    internal class CustomerBO : ViewModelBase, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.PlatformCentralizeQuery.LinqOP;

        #region 属性

        private int _iD;
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

        private bool _isHold;
        /// <summary>
        /// 是否已被选择-用于界面绑定
        /// </summary>
        //internal的话前台绑定就不能反馈，telerik的bug？
        public bool IsHold
        {
            get { return _isHold; }
            set
            {
                if (_isHold != value)
                {
                    _isHold = value;
                    OnPropertyChanged("IsHold");
                }
            }
        }

        private List<SoftToUpdateBO> _softs;
        public List<SoftToUpdateBO> Softs
        {
            get
            {
                if (_softs == null)
                {
                    var softs = _linqOP.GetDataContext<SoftToUpdate>();
                    var mappings = _linqOP.Search<SoftCustomerMapping>(o => o.CustomerID == this.ID);
                    var query = from s in softs
                                from map in mappings
                                where s.ID == map.SoftID
                                select s;
                    _softs = query.Select(o => (SoftToUpdateBO)o).ToList();
                    _softs.ForEach(o =>
                    {
                        var version = VersionTracks.FirstOrDefault(v => v.SoftID == o.ID);
                        o.CurrentVersion = version == null ? "" : version.VersionCode;
                    });
                }
                return _softs;
            }
        }

        private ObservableCollection<SoftVersionTrackBO> _versionTracks;
        internal ObservableCollection<SoftVersionTrackBO> VersionTracks
        {
            get
            {
                if (_versionTracks == null)
                {
                    var tracks = _linqOP.GetDataContext<SoftVersionTrack>();
                    var mappings = _linqOP.Search<SoftVersionCustomerMapping>(o => o.CustomerID == this.ID);
                    var query = from t in tracks
                                from map in mappings
                                where t.ID == map.SoftVersionID
                                select t;
                    var data = query.Select(o => (SoftVersionTrackBO)o).ToList().OrderByDescending(o => o.CreateTime);
                    _versionTracks = new ObservableCollection<SoftVersionTrackBO>(data);
                }
                return _versionTracks;
            }
        }

        #endregion

        public CustomerBO()
        { }

        internal CustomerBO(Customer customer)
        {
            this.ID = customer.ID;
            this.IdentificationKey = customer.IdentificationKey;
            this.Address = customer.Address;
            this.Linkman = customer.Linkman;
            this.Phone = customer.Phone;
            this.Name = customer.Name;
            this.UserPointLimit = customer.UserPointLimit;
            this.ApiUrls = customer.ApiUrls;
        }

        #region 类型转换操作符重载

        public static implicit operator CustomerBO(Customer customer)
        {
            return new CustomerBO(customer);
        }

        public static implicit operator Customer(CustomerBO customer)
        {
            return new Customer
            {
                ID = customer.ID,
                IdentificationKey = customer.IdentificationKey,
                Name = customer.Name,
                Address = customer.Address,
                Phone = customer.Phone,
                Linkman = customer.Linkman,
                UserPointLimit = customer.UserPointLimit,
                ApiUrls = customer.ApiUrls
            };
        }

        #endregion

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name")
            {
                if (string.IsNullOrWhiteSpace(Name))
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<Customer>(e => e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<Customer>(e => e.ID != ID && e.Name == Name))
                        errorInfo = "该名称已经被使用";
                }
            }

            return errorInfo;
        }

        string IDataErrorInfo.Error
        {
            get { return ""; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return this.CheckData(columnName);
            }
        }
    }
}
