using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace SysProcessModel
{
    //SysModule
    public class SysModule : IDEntity
    {
        /// <summary>
        /// ID
        /// </summary>		
        private int _id;
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// Name
        /// </summary>		
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// ApplicationId
        /// </summary>		
        private int _applicationid;
        public int ApplicationId
        {
            get { return _applicationid; }
            set { _applicationid = value; }
        }
        /// <summary>
        /// Code
        /// </summary>		
        private string _code;
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }
        /// <summary>
        /// ParentCode
        /// </summary>		
        private string _parentcode;
        public string ParentCode
        {
            get { return _parentcode; }
            set { _parentcode = value; }
        }
        /// <summary>
        /// Uri
        /// </summary>		
        private string _uri;
        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        public string MobileUri { get; set; }
    }
}

