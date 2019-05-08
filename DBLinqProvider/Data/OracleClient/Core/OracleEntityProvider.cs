using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DBLinqProvider.Data.OracleCore
{
    using DBLinqProvider.Data.Common;
    
    public class OracleEntityProvider : DbEntityProvider
    {        
        //bool? allowMulitpleActiveResultSets;

        public OracleEntityProvider(DbConnection connection, QueryMapping mapping, QueryPolicy policy)
            : base(connection, PLSqlLanguage.Default, mapping, policy)
        {            
        }
        
        public bool AllowsMultipleActiveResultSets
        {
            get
            {
                return false;
                //if (this.allowMulitpleActiveResultSets == null)
                //{
                //    var builder = new OracleConnectionStringBuilder(this.Connection.ConnectionString);
                //    var result = builder.ContainsKey("MultipleActiveResultSets") ? builder["MultipleActiveResultSets"] : null;
                //    this.allowMulitpleActiveResultSets = (result != null && result.GetType() == typeof(bool) && (bool)result);
                //}
                //return (bool)this.allowMulitpleActiveResultSets;
            }
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new OracleExecutor(this);
        }        

        public virtual AdoProvider AdoProvider
        {
            get
            {
                throw new NotImplementedException("AdoProvider need to be implemented.");
            }
        }
       
    }
}
