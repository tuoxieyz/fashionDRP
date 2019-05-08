using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using DBLinqProvider.Data.OracleCore;

namespace DBLinqProvider.Data.OracleClient
{
    public class AdoOracleClientProvider : AdoProvider
    {
        public AdoOracleClientProvider()
        {
            _dbConnectionTypeName = "System.Data.OracleClient.OracleConnection";
            _dbTypeTypeName = "System.Data.OracleClient.OracleType";
            _dbParameterTypeName = "System.Data.OracleClient.OracleParameter";
            _dbDataAdapterTypeName = "System.Data.OracleClient.OracleDataAdapter";
            _dbTypePropertyName = "OracleType";

            _oracleTypeMapping.Add(SqlDbType.BigInt, GetOracleType("Number"));
            _oracleTypeMapping.Add(SqlDbType.Binary, GetOracleType("Blob"));
            _oracleTypeMapping.Add(SqlDbType.Bit, GetOracleType("Byte"));
            _oracleTypeMapping.Add(SqlDbType.NChar, GetOracleType("NChar"));
            _oracleTypeMapping.Add(SqlDbType.Char, GetOracleType("Char"));
            _oracleTypeMapping.Add(SqlDbType.Date, GetOracleType("DateTime"));
            _oracleTypeMapping.Add(SqlDbType.DateTime, GetOracleType("DateTime"));
            _oracleTypeMapping.Add(SqlDbType.SmallDateTime, GetOracleType("DateTime"));
            _oracleTypeMapping.Add(SqlDbType.Decimal, GetOracleType("Number"));
            _oracleTypeMapping.Add(SqlDbType.Float, GetOracleType("Float"));
            _oracleTypeMapping.Add(SqlDbType.Image, GetOracleType("Blob"));
            _oracleTypeMapping.Add(SqlDbType.Int, GetOracleType("Int32"));
            _oracleTypeMapping.Add(SqlDbType.Money, GetOracleType("Number"));
            _oracleTypeMapping.Add(SqlDbType.SmallMoney, GetOracleType("Number"));
            _oracleTypeMapping.Add(SqlDbType.NVarChar, GetOracleType("NVarChar"));
            _oracleTypeMapping.Add(SqlDbType.VarChar, GetOracleType("VarChar"));
            _oracleTypeMapping.Add(SqlDbType.SmallInt, GetOracleType("Int16"));
            _oracleTypeMapping.Add(SqlDbType.NText, GetOracleType("Clob"));
            _oracleTypeMapping.Add(SqlDbType.Text, GetOracleType("Clob"));
            _oracleTypeMapping.Add(SqlDbType.Time, GetOracleType("Timestamp"));
            _oracleTypeMapping.Add(SqlDbType.Timestamp, GetOracleType("Timestamp"));
            _oracleTypeMapping.Add(SqlDbType.TinyInt, GetOracleType("Byte"));
            _oracleTypeMapping.Add(SqlDbType.UniqueIdentifier, GetOracleType("VarChar"));
            _oracleTypeMapping.Add(SqlDbType.VarBinary, GetOracleType("Blob"));
            _oracleTypeMapping.Add(SqlDbType.Xml, GetOracleType("Clob"));

        }

        public override Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                {
                    if (Environment.Version.Major < 4)
                    {
                        _assembly = Assembly.Load("System.Data.OracleClient, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    }
                    else if (Environment.Version.Major == 4)
                    {
                        _assembly = Assembly.Load("System.Data.OracleClient, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    }
                }
                return _assembly;
            }
        }


        private static AdoOracleClientProvider _default;
        public static AdoOracleClientProvider Default
        {
            get
            {
                if (_default == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _default, new AdoOracleClientProvider(), null);
                }
                return _default;
            }
        }

        
    }

}
