using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider;
using DBLinqProvider.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Kernel;
using System.Data.Common;
using System.Web.Configuration;

namespace DBAccess
{
    public class QueryGlobal
    {
        private static Dictionary<string, QueryGlobal> _dic = new Dictionary<string, QueryGlobal>();

        const string QueryProviderStr = "DBLinqProvider.Data.SqlClient.SqlQueryProvider,DBLinqProvider";

        public IEntityProvider QueryProvider { get; set; }

        public LinqOPEncap LinqOP { get; set; }

        /// <summary>
        /// 微软企业库之ADO操作封装
        /// </summary>
        public Database DB { get; private set; }

        public QueryGlobal(string constrName)//, bool isWeb = false
        {
            if (!_dic.ContainsKey(constrName))
            {
                //isWeb ? WebConfigurationManager.ConnectionStrings[constrName].ConnectionString :
                //似乎在web程序中使用ConfigurationManager也能读到连接字符串
                var connectionString =  ConfigurationManager.ConnectionStrings[constrName].ConnectionString;
                connectionString = this.DecryptConnectionString(connectionString);//new DESCrypt().DecryptDES(connectionString);
                QueryProvider = DbEntityProvider.From(QueryProviderStr, connectionString);
                LinqOP = new LinqOPEncap(QueryProvider);
                DB = new SqlDatabase(connectionString);
                _dic.Add(constrName, this);
            }
            else
            {
                var cache = _dic[constrName];
                QueryProvider = cache.QueryProvider;
                LinqOP = cache.LinqOP;
                DB = cache.DB;
            }
        }

        private string DecryptConnectionString(string connectionString)
        {
            var descrypt = new DESCrypt();
            DbConnectionStringBuilder connSb = new DbConnectionStringBuilder();
            connSb.ConnectionString = connectionString;
            if (connSb.ContainsKey("pwd"))
                connSb["pwd"] = descrypt.DecryptDES(connSb["pwd"].ToString());
            else if (connSb.ContainsKey("password"))
                connSb["password"] = descrypt.DecryptDES(connSb["password"].ToString());
            connSb["Server"] = descrypt.DecryptDES(connSb["Server"].ToString());
            connSb["user"] = descrypt.DecryptDES(connSb["user"].ToString());
            return connSb.ConnectionString;
        }
    }
}
