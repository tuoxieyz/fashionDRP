using Kernel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextExtension
{
    public class AdapDbContext : DbContext
    {
        private static Dictionary<string, string> _dicEntityConnString = new Dictionary<string, string>();

        public AdapDbContext(string constrName, string efMetadata)
            : base(GetEntityConnString(constrName, efMetadata))
        { }

        private static string GetEntityConnString(string constrName, string efMetadata)
        {
            if (!_dicEntityConnString.ContainsKey(constrName))
            {
                var connectionString = ConfigurationManager.ConnectionStrings[constrName].ConnectionString;
                connectionString = DecryptConnectionString(connectionString);

                EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
                //Metadata属性的值，是从向导生成的Config粘贴过来的
                entityBuilder.Metadata = efMetadata;//"res://*/SysProcess.csdl|res://*/SysProcess.ssdl|res://*/SysProcess.msl";
                entityBuilder.ProviderConnectionString = connectionString;
                entityBuilder.Provider = "System.Data.SqlClient";

                _dicEntityConnString.Add(constrName, entityBuilder.ToString());
            }
            return _dicEntityConnString[constrName];
        }

        private static string DecryptConnectionString(string connectionString)
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
