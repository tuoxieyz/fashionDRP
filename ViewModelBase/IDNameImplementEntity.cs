using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;

namespace ViewModelBasic
{
    public class IDNameImplementEntity : IDNameEntity
    {
        public string Name
        {
            get;
            set;
        }

        public int ID
        {
            get;
            set;
        }
    }
}
