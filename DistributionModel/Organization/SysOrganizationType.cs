using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using DBAccess;

namespace ModelEntity.Organization
{
    public class SysOrganizationType : OrganizationHierarchyEntityWithIDName
    {
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        public SysOrganizationType()
        {
            Query = new QueryGlobal("SysProcessConstr");
        }
    }
}
