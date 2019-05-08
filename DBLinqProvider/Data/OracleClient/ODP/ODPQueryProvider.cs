using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using DBLinqProvider;
using DBLinqProvider.Data.Common;
using DBLinqProvider.Data.OracleCore;

namespace DBLinqProvider.Data.ODP 
{    

    public class ODPQueryProvider : OracleEntityProvider 
    {
        public ODPQueryProvider(DbConnection connection, QueryMapping mapping, QueryPolicy policy)
            : base(connection, mapping, policy)
        {            
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new ODPExecutor(this);
        }    

        public override AdoProvider AdoProvider
        {
            get
            {
                return AdoOracleDataProvider.Default;
            }
        }

        public static Type AdoConnectionType
        {
            get
            {
                return AdoOracleDataProvider.Default.DbConnectionType;
            }
        }

    }
}
