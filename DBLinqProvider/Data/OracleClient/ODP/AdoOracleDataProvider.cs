using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.IO;
using DBLinqProvider.Data.OracleCore;

namespace DBLinqProvider.Data.ODP
{
    public class AdoOracleDataProvider : AdoProvider
    {
        public AdoOracleDataProvider()
        {
            _dbConnectionTypeName = "Oracle.DataAccess.Client.OracleConnection";
            _dbTypeTypeName = "Oracle.DataAccess.Client.OracleDbType";
            _dbParameterTypeName = "Oracle.DataAccess.Client.OracleParameter";
            _dbDataAdapterTypeName = "Oracle.DataAccess.Client.OracleDataAdapter";
            _dbTypePropertyName = "OracleDbType";

            _oracleTypeMapping.Add(SqlDbType.BigInt, GetOracleType("Int64"));
            _oracleTypeMapping.Add(SqlDbType.Binary, GetOracleType("Blob"));
            _oracleTypeMapping.Add(SqlDbType.Bit, GetOracleType("Byte"));
            _oracleTypeMapping.Add(SqlDbType.NChar, GetOracleType("NChar"));
            _oracleTypeMapping.Add(SqlDbType.Char, GetOracleType("Char"));
            _oracleTypeMapping.Add(SqlDbType.Date, GetOracleType("Date"));
            _oracleTypeMapping.Add(SqlDbType.DateTime, GetOracleType("Date"));
            _oracleTypeMapping.Add(SqlDbType.SmallDateTime, GetOracleType("Date"));
            _oracleTypeMapping.Add(SqlDbType.Decimal, GetOracleType("Decimal"));
            _oracleTypeMapping.Add(SqlDbType.Float, GetOracleType("Single"));
            _oracleTypeMapping.Add(SqlDbType.Image, GetOracleType("Blob"));
            _oracleTypeMapping.Add(SqlDbType.Int, GetOracleType("Int32"));
            _oracleTypeMapping.Add(SqlDbType.Money, GetOracleType("Decimal"));
            _oracleTypeMapping.Add(SqlDbType.SmallMoney, GetOracleType("Decimal"));
            _oracleTypeMapping.Add(SqlDbType.NVarChar, GetOracleType("NVarchar2"));
            _oracleTypeMapping.Add(SqlDbType.VarChar, GetOracleType("Varchar2"));
            _oracleTypeMapping.Add(SqlDbType.SmallInt, GetOracleType("Int16"));
            _oracleTypeMapping.Add(SqlDbType.NText, GetOracleType("Clob"));
            _oracleTypeMapping.Add(SqlDbType.Text, GetOracleType("Clob"));
            _oracleTypeMapping.Add(SqlDbType.Time, GetOracleType("TimeStamp"));
            _oracleTypeMapping.Add(SqlDbType.Timestamp, GetOracleType("TimeStamp"));
            _oracleTypeMapping.Add(SqlDbType.TinyInt, GetOracleType("Byte"));
            _oracleTypeMapping.Add(SqlDbType.UniqueIdentifier, GetOracleType("Varchar2"));
            _oracleTypeMapping.Add(SqlDbType.VarBinary, GetOracleType("Blob"));
            _oracleTypeMapping.Add(SqlDbType.Xml, GetOracleType("XmlType"));

        }

        public override Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                {
                    //_assembly = Assembly.Load("Oracle.DataAccess, Version=2.111.7.20, Culture=neutral, PublicKeyToken=89b483f429c47342");

                    //LoadWithPartialName still in .Net 4.0
#pragma warning disable 618
                    _assembly = Assembly.LoadWithPartialName("Oracle.DataAccess");
#pragma warning restore 618
                }
                return _assembly;
            }
        }        
        
        private static AdoOracleDataProvider _default;
        public static AdoOracleDataProvider Default
        {
            get
            {
                if (_default == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _default, new AdoOracleDataProvider(), null);
                }
                return _default;
            }
        }
        
    }

}
