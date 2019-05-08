using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace UpdateOnline.Extension
{
    //[DataContract(Namespace = "http://www.tuoxie.com/version/")]
    public class FilesNeedUpdate
    {
        //[DataMember]
        public FileNeedUpdate[] Files { get; set; }
        //[DataMember]
        public FileNeedUpdate[] Directories { get; set; }
        //[DataMember]
        public bool IsEmpty { get {
            return (Files == null || Files.Length == 0) && (Directories == null || Directories.Length == 0);
        }
            //private set { }//作为wcf的DataMember属性字段必须得同时有get和set
        }
    }

    public class FileNeedUpdate
    {
        /// <summary>
        /// 文件名称(路径),不包含根路径信息
        /// </summary>
        public string Name { get; set; }

        private bool _isDelete = false;

        public bool IsDelete
        {
            get { return _isDelete; }
            set { _isDelete = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
