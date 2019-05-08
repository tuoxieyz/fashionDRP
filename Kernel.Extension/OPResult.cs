using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Kernel
{
    /// <summary>
    /// 通用操作结果
    /// </summary>
    [DataContract]
    public class OPResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        [DataMember]
        public bool IsSucceed { get; set; }

        private string _message = "保存成功";

        /// <summary>
        /// 操作的结果说明
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    public class OPResult<TResult> : OPResult
    {
        [DataMember]
        public TResult Result { get; set; }
    }
}