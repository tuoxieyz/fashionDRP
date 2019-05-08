using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace IWCFServiceForIM
{
    [DataContract]
    public class IMessage
    {
        [DataMember]
        public string Message { get; set; }
        //[DataMember]
        //public string SenderName { get; set; }

        [DataMember]
        public UserPoint Sender { get; set; }

        private DateTime? _sendTime = null;
        [DataMember]
        public DateTime SendTime
        {
            get
            {
                return this._sendTime.HasValue
                   ? this._sendTime.Value
                   : DateTime.Now;
            }

            set { this._sendTime = value; }
        }

        /// <summary>
        /// 0:用户消息 1:系统消息
        /// </summary>
        [DataMember]
        public int MessageKind { get; set; }
    }
}
