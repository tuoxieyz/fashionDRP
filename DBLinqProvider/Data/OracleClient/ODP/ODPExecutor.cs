using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;
using DBLinqProvider.Data.Common;
using DBLinqProvider.Data.OracleCore;

namespace DBLinqProvider.Data.ODP
{
    public class ODPExecutor : OracleExecutor
    {
        public ODPExecutor(OracleEntityProvider provider)
            : base(provider)
        {            
        }

        protected override DbCommand GetCommand(QueryCommand query, object[] paramValues)
        {
            DbCommand cmd = base.GetCommand(query, paramValues);

            if (_bindByNameProperty == null)
            {
                _bindByNameProperty = cmd.GetType().GetProperty("BindByName");
                //_notificationAutoEnlistProperty = cmd.GetType().GetProperty("NotificationAutoEnlist");
            }
            _bindByNameProperty.SetValue(cmd, true);
            //_notificationAutoEnlistProperty.SetValue(cmd, false);            

            return cmd;
        }

        
        static PropertyInfo _bindByNameProperty;
        //static PropertyInfo _notificationAutoEnlistProperty;

    }
}
