using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
namespace SysProcessModel
{
    //SysUserModule
    public class SysUserModule
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
        /// ModuleId
        /// </summary>		
        private int _moduleid;
        public int ModuleId
        {
            get { return _moduleid; }
            set { _moduleid = value; }
        }
        /// <summary>
        /// CreatorID
        /// </summary>		
        private int _creatorcode;
        public int CreatorID
        {
            get { return _creatorcode; }
            set { _creatorcode = value; }
        }
        /// <summary>
        /// CreateTime
        /// </summary>		
        private DateTime _createtime;
        public DateTime CreateTime
        {
            get { return _createtime; }
            set { _createtime = value; }
        }

    }
}

