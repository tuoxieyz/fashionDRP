using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using System.ComponentModel;
using System.Linq;

namespace SysProcessModel
{
    //ProStyle
    public class ProSCPicture
    {
        //[ColumnAttribute(IsPrimaryKey = true)]
        //public string SCCode { get; set; }
        public int StyleID { get; set; }
        public int ColorID { get; set; }
        //public int BYQID { get; set; }
        public string PictureName { get; set; }
        /// <summary>
        /// 上传日期
        /// </summary>
        public DateTime UploadTime { get; set; }
    }
}

