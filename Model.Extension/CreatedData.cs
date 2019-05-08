using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Model.Extension
{
    public class CreatedData
    {
        public int CreatorID { get; set; }

        private DateTime _createTime = DateTime.Now;
        public DateTime CreateTime { get { return _createTime; } set { _createTime = value; } }
    }
}
