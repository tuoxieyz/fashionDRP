using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;

namespace VersionManager
{
    internal static class VMGlobal
    {
        internal static readonly QueryGlobal PlatformCentralizeQuery = new QueryGlobal("PlatformCentralizeConnection");
    }
}
