using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using DBLinqProvider.Data.Common;

namespace DBLinqProvider.Data.OracleCore
{
    public class OracleExecutor : DbEntityProvider.Executor
    {
        OracleEntityProvider provider;

        public OracleExecutor(OracleEntityProvider provider)
            : base(provider)
        {
            this.provider = provider;
        }

        protected override bool BufferResultRows
        {
            get { return !this.provider.AllowsMultipleActiveResultSets; }
        }

        protected override void AddParameter(DbCommand command, QueryParameter parameter, object value)
        {
            DbQueryType sqlType = (DbQueryType)parameter.QueryType;
            if (sqlType == null)
                sqlType = (DbQueryType)this.Provider.Language.TypeSystem.GetColumnType(parameter.Type);
            int len = sqlType.Length;
            if (len == 0 && DbTypeSystem.IsVariableLength(sqlType.SqlDbType))
            {
                len = Int32.MaxValue;
            }
            //var p = ((OracleCommand)command).Parameters.Add(":" + parameter.Name, ToOracleType(sqlType.SqlDbType), len);                
            var p = provider.AdoProvider.CreateParameter(":" + parameter.Name, sqlType.SqlDbType, len);
            /*
            if (sqlType.Precision != 0)
                p.Precision = (byte)sqlType.Precision;
            if (sqlType.Scale != 0)
                p.Scale = (byte)sqlType.Scale;
            */
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        public override IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets, int batchSize, bool stream)
        {
            this.StartUsingConnection();
            try
            {
                var result = this.ExecuteBatch(query, paramSets, batchSize);
                if (!stream || this.ActionOpenedConnection)
                {
                    return result.ToList();
                }
                else
                {
                    return new EnumerateOnce<int>(result);
                }
            }
            finally
            {
                this.StopUsingConnection();
            }
        }

        private IEnumerable<int> ExecuteBatch(QueryCommand query, IEnumerable<object[]> paramSets, int batchSize)
        {
            DbCommand cmd = this.GetCommand(query, null);
            DataTable dataTable = new DataTable();
            for (int i = 0, n = query.Parameters.Count; i < n; i++)
            {
                var qp = query.Parameters[i];
                cmd.Parameters[i].SourceColumn = qp.Name;
                dataTable.Columns.Add(qp.Name, TypeHelper.GetNonNullableType(qp.Type));
            }
            DbDataAdapter dataAdapter = provider.AdoProvider.CreateDataAdapter();
            dataAdapter.InsertCommand = cmd;
            dataAdapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
            dataAdapter.UpdateBatchSize = batchSize;

            this.LogMessage("-- Start SQL Batching --");
            this.LogMessage("");
            this.LogCommand(query, null);

            IEnumerator<object[]> en = paramSets.GetEnumerator();
            using (en)
            {
                bool hasNext = true;
                while (hasNext)
                {
                    int count = 0;
                    for (; count < dataAdapter.UpdateBatchSize && (hasNext = en.MoveNext()); count++)
                    {
                        var paramValues = en.Current;
                        dataTable.Rows.Add(paramValues);
                        this.LogParameters(query, paramValues);
                        this.LogMessage("");
                    }
                    if (count > 0)
                    {
                        int n = dataAdapter.Update(dataTable);
                        for (int i = 0; i < count; i++)
                        {
                            yield return (i < n) ? 1 : 0;
                        }
                        dataTable.Rows.Clear();
                    }
                }
            }

            this.LogMessage(string.Format("-- End SQL Batching --"));
            this.LogMessage("");
        }


    }//end of Executor
}
