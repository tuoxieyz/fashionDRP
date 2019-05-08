using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CentralizeModel;
using DBAccess;
using Telerik.Windows.Controls;

namespace VersionManager.BO
{
    internal class SoftVersionTrackBO : ViewModelBase, IDataErrorInfo
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

        private string _versionCode;
        public string VersionCode
        {
            get
            {
                return this._versionCode;
            }
            set
            {
                this._versionCode = value;
                OnPropertyChanged("VersionCode");
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        private string _updatedFileList;
        public string UpdatedFileList
        {
            get
            {
                return this._updatedFileList;
            }
            set
            {
                this._updatedFileList = value;
            }
        }

        private bool _isCoerciveUpdate = true;
        public bool IsCoerciveUpdate
        {
            get
            {
                return this._isCoerciveUpdate;
            }
            set
            {
                this._isCoerciveUpdate = value;
                OnPropertyChanged("IsCoerciveUpdateStr");
            }
        }

        private DateTime _createTime;
        public DateTime CreateTime
        {
            get
            {
                return this._createTime;
            }
            set
            {
                this._createTime = value;
            }
        }

        public string IsCoerciveUpdateStr { get { return IsCoerciveUpdate ? "是" : "否"; } }

        private List<CustomerBO> _customers;
        public List<CustomerBO> Customers
        {
            get
            {
                if (_customers == null)
                {
                    if (this.ID == default(int))
                    {
                        _customers = new List<CustomerBO>();
                    }
                    else
                    {
                        var customers = _linqOP.GetDataContext<Customer>();
                        var mappings = _linqOP.Search<SoftVersionCustomerMapping>(o => o.SoftVersionID == this.ID);
                        var query = from c in customers
                                    from map in mappings
                                    where c.ID == map.CustomerID
                                    select c;
                        _customers = query.Select(o => (CustomerBO)o).ToList();
                    }
                }
                return _customers;
            }
            set
            {
                _customers = value;
            }
        }

        private SoftToUpdateBO _soft;
        public SoftToUpdateBO Soft
        {
            get
            {
                if (_soft == null)
                {
                    _soft = _linqOP.GetById<SoftToUpdate>(this.SoftID);
                }
                return _soft;
            }
            set
            {
                _soft = value;
            }
        }

        #endregion

        public SoftVersionTrackBO()
        { }

        internal SoftVersionTrackBO(SoftVersionTrack softversion)
        {
            this.ID = softversion.ID;
            this.SoftID = softversion.SoftID;
            this.IsCoerciveUpdate = softversion.IsCoerciveUpdate;
            this.UpdatedFileList = softversion.UpdatedFileList;
            this.VersionCode = softversion.VersionCode;
            this.Description = softversion.Description;
            this.CreateTime = softversion.CreateTime;
        }

        #region 类型转换操作符重载

        public static implicit operator SoftVersionTrackBO(SoftVersionTrack softversion)
        {
            return new SoftVersionTrackBO(softversion);
        }

        public static implicit operator SoftVersionTrack(SoftVersionTrackBO softversion)
        {
            return new SoftVersionTrack
            {
                ID = softversion.ID,
                IsCoerciveUpdate = softversion.IsCoerciveUpdate,
                UpdatedFileList = softversion.UpdatedFileList,
                VersionCode = softversion.VersionCode,
                Description = softversion.Description,
                CreateTime = softversion.CreateTime,
                SoftID = softversion.SoftID
            };
        }

        #endregion

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "VersionCode")
            {
                if (string.IsNullOrWhiteSpace(VersionCode))
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<SoftVersionTrack>(e => e.SoftID == this.SoftID && e.VersionCode == VersionCode))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<SoftVersionTrack>(e => e.SoftID == this.SoftID && e.ID != ID && e.VersionCode == VersionCode))
                        errorInfo = "该名称已经被使用";
                }
            }
            else if (columnName == "UpdatedFileList")
            {
                if (string.IsNullOrWhiteSpace(UpdatedFileList))
                    errorInfo = "不能为空";
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
