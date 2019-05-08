using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace SysProcessModel
{
    //SysUserRole
    public class SysUserRole : CreatedData
    {
        /// <summary>
        /// UserId
        /// </summary>		
        private int _userid;
        public int UserId
        {
            get { return _userid; }
            set { _userid = value; }
        }
        /// <summary>
        /// RoleId
        /// </summary>		
        private int _roleid;
        public int RoleId
        {
            get { return _roleid; }
            set { _roleid = value; }
        }
    }
}

