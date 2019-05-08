using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Model.Extension;
namespace SysProcessModel
{
    //ProStyleMatching
    public class ProStyleMatching : CreatedData
    {
        public int GroupID { get; set; }
        public int StyleID { get; set; }
        public int ColorID { get; set; }
    }
}

