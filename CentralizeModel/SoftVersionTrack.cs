using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace CentralizeModel
{
    public class SoftVersionTrack
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
        public virtual string VersionCode
        {
            get
            {
                return this._versionCode;
            }
            set
            {
                this._versionCode = value;
            }
        }

        private string _description;
        public virtual string Description
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

        private bool _isCoerciveUpdate;
        public virtual bool IsCoerciveUpdate
        {
            get
            {
                return this._isCoerciveUpdate;
            }
            set
            {
                this._isCoerciveUpdate = value;
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
    }
}
