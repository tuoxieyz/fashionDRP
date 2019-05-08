using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using System.ComponentModel;
using CentralizeModel;
using Kernel;
using System.Transactions;
using System.Collections.ObjectModel;
using ViewModelBasic;

namespace VersionManager.BO
{
    internal class SoftToUpdateBO : IDataErrorInfo, ICUDOper<SoftVersionTrackBO>
    {
        private LinqOPEncap _linqOP = VMGlobal.PlatformCentralizeQuery.LinqOP;

        public int ID
        {
            get;
            set;
        }

        public string IdentificationKey { get; set; }

        private string _softName;
        public string SoftName
        {
            get
            {
                return this._softName;
            }
            set
            {
                this._softName = value;
            }
        }

        public string DownloadUrl { get; set; }

        public string UpdateUrl { get; set; }

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
                        var mappings = _linqOP.Search<SoftCustomerMapping>(o => o.SoftID == this.ID);
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

        private ObservableCollection<SoftVersionTrackBO> _versionTracks;
        public ObservableCollection<SoftVersionTrackBO> VersionTracks
        {
            get
            {
                if (_versionTracks == null)
                {
                    var data = _linqOP.Search<SoftVersionTrack>(o => o.SoftID == this.ID).Select(o => (SoftVersionTrackBO)o).ToList().OrderByDescending(o => o.CreateTime);
                    foreach (var d in data)
                    {
                        d.Soft = this;
                    }
                    _versionTracks = new ObservableCollection<SoftVersionTrackBO>(data);
                }
                return _versionTracks;
            }
        }

        public string CurrentVersion { get; set; }

        //要有无参构造器，否则UI层不晓得怎么动态实例化对象
        //对于Telerik控件来说，还必须是public的构造器
        public SoftToUpdateBO()
        { }

        internal SoftToUpdateBO(SoftToUpdate soft)
        {
            this.ID = soft.ID;
            this.IdentificationKey = soft.IdentificationKey;
            this.SoftName = soft.SoftName;
            this.DownloadUrl = soft.DownloadUrl;
            this.UpdateUrl = soft.UpdateUrl;
            this.Description = soft.Description;
        }

        #region 类型转换操作符重载

        public static implicit operator SoftToUpdateBO(SoftToUpdate soft)
        {
            return new SoftToUpdateBO(soft);
        }

        public static implicit operator SoftToUpdate(SoftToUpdateBO soft)
        {
            return new SoftToUpdate
            {
                ID = soft.ID,
                IdentificationKey = soft.IdentificationKey,
                SoftName = soft.SoftName,
                DownloadUrl = soft.DownloadUrl,
                UpdateUrl = soft.UpdateUrl,
                Description = soft.Description
            };
        }

        #endregion

        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "SoftName")
            {
                if (string.IsNullOrWhiteSpace(SoftName))
                    errorInfo = "不能为空";
                else if (ID == 0)//新增
                {
                    if (_linqOP.Any<SoftToUpdate>(e => e.SoftName == SoftName))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<SoftToUpdate>(e => e.ID != ID && e.SoftName == SoftName))
                        errorInfo = "该名称已经被使用";
                }
            }
            else if (columnName == "DownloadUrl")
            {
                if (string.IsNullOrWhiteSpace(DownloadUrl))
                    errorInfo = "不能为空";
            }
            else if (columnName == "UpdateUrl")
            {
                if (string.IsNullOrWhiteSpace(UpdateUrl))
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

        public OPResult AddOrUpdate(SoftVersionTrackBO version)
        {
            int id = version.ID;//临时变量，当保存出错时还原
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    if (id == default(int))
                    {
                        version.CreateTime = DateTime.Now;
                        version.SoftID = this.ID;
                        version.ID = _linqOP.Add<SoftVersionTrack, int>(version, o => o.ID);
                    }
                    else
                    {
                        _linqOP.Update<SoftVersionTrack>(version);
                        _linqOP.Delete<SoftVersionCustomerMapping>(o => o.SoftVersionID == version.ID);
                    }
                    var mapping = version.Customers.Select(o => new SoftVersionCustomerMapping { SoftVersionID = version.ID, CustomerID = o.ID });
                    _linqOP.Add(mapping);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    version.ID = id;
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
            if (id == default(int))
            {
                this.VersionTracks.Insert(0, version);
            }
            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        public OPResult Delete(SoftVersionTrackBO version)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    _linqOP.Delete<SoftVersionCustomerMapping>(o => o.SoftVersionID == version.ID);
                    _linqOP.Delete<SoftVersionTrack>(version);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
                }
            }
            this.VersionTracks.Remove(version);
            return new OPResult { IsSucceed = true, Message = "删除成功." };
        }
    }
}
