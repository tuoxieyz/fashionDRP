using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace DBLinqProvider.Data.OracleCore
{
    public class AdoProvider
    {
        protected Assembly _assembly;
        public virtual Assembly Assembly
        {
            get
            {
                return _assembly;
            }
        }

        protected Type _dbConnectionType;
        protected string _dbConnectionTypeName;

        public virtual Type DbConnectionType
        {
            get
            {
                if (_dbConnectionType == null)
                {
                    _dbConnectionType = this.Assembly.GetType(_dbConnectionTypeName);
                }
                return _dbConnectionType;
            }
        }

        protected Type _dbTypeType;
        protected string _dbTypeTypeName;
        public virtual Type DbTypeType
        {
            get
            {
                if (_dbTypeType == null)
                {
                    _dbTypeType = this.Assembly.GetType(_dbTypeTypeName);
                }
                return _dbTypeType;
            }
        }

        protected Type _dbParameterType;
        protected string _dbParameterTypeName;
        public virtual Type DbParameterType
        {
            get
            {
                if (_dbParameterType == null)
                {
                    _dbParameterType = this.Assembly.GetType(_dbParameterTypeName);
                }
                return _dbParameterType;
            }
        }

        protected Type _dbDataAdapterType;
        protected string _dbDataAdapterTypeName;
        public virtual Type DbDataAdapterType
        {
            get
            {
                if (_dbDataAdapterType == null)
                {
                    _dbDataAdapterType = this.Assembly.GetType(_dbDataAdapterTypeName);
                }
                return _dbDataAdapterType;
            }
        }


        protected PropertyInfo _dbTypeProperty;
        protected string _dbTypePropertyName;
        public virtual PropertyInfo DbTypeProperty
        {
            get
            {
                if (_dbTypeProperty == null)
                {
                    _dbTypeProperty = this.DbParameterType.GetProperty(_dbTypePropertyName);
                }
                return _dbTypeProperty;
            }
        }

        public virtual int GetOracleType(string name)
        {
            Type oracleType = this.DbTypeType;
            FieldInfo fi = oracleType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            object v = fi.GetValue(null);
            return Convert.ToInt32(v);
        }

        protected Dictionary<SqlDbType, int> _oracleTypeMapping = new Dictionary<SqlDbType,int>();
        public virtual int ToOracleType(SqlDbType dbType)
        {
            if (_oracleTypeMapping.ContainsKey(dbType))
            {
                return _oracleTypeMapping[dbType];
            }
            else
            {
                throw new NotSupportedException(string.Format("The SQL type '{0}' is not supported", dbType));
            }
        }

        public virtual DbParameter CreateParameter(string name, SqlDbType dbType, int size)
        {
            DbParameter p = (DbParameter)Activator.CreateInstance(this.DbParameterType);
            p.ParameterName = name;
            this.DbTypeProperty.SetValue(p, ToOracleType(dbType));
            p.Size = size;
            return p;
        }

        public virtual DbDataAdapter CreateDataAdapter()
        {
            DbDataAdapter da = (DbDataAdapter)Activator.CreateInstance(this.DbDataAdapterType);
            return da;
        }
    }
}
