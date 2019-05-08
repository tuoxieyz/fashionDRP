using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateOnline
{
    internal class BackUpException : Exception
    {
        public BackUpException():base("文件备份出错")
        {
        }
        public BackUpException(string message)
            : base(message)
        {
        }
        public BackUpException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
