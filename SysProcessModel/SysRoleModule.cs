using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace SysProcessModel
{
    //SysRoleModule
    public class SysRoleModule
    {
        /// <summary>
        /// RoleId
        /// </summary>		
        private int _roleid;
        public int RoleId
        {
            get { return _roleid; }
            set { _roleid = value; }
        }
        /// <summary>
        /// ModuleId
        /// </summary>		
        private int _moduleid;
        public int ModuleId
        {
            get { return _moduleid; }
            set { _moduleid = value; }
        }

    }
}

