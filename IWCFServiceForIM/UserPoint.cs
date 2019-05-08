using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;

namespace IWCFServiceForIM
{
    /// <summary>
    /// 用户终端
    /// </summary>
    [DataContract]
    public class UserPoint
    {
        /// <summary>
        /// 用户标识
        /// <remarks>原打算使用业务系统用户ID，但考虑到用户多点登录（甚至变态的一机多点）的情况，避免将自己也踢下线,改为使用GUID;还有防止多次打洞的好处</remarks>
        /// </summary>
        [DataMember]
        public string UserGuid { get; set; }

        [DataMember]
        public int UserID { get; set; }
        [DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// 用户所在机构ID
        /// </summary>
        [DataMember]
        public int OrganizationID { get; set; }
        [DataMember]
        public string OrganizationName { get; set; }

        /// <summary>
        /// 用户主机用于侦听和发送消息的网络地址(和端口)
        /// <remarks>当同一台机子运行多个客户端时共用同一个端口，ERP和聊天软件不同，消息推送并不知道准确的接收方是谁，因此不需要通过端口区分不同客户端。
        /// 是否接收业务消息确切的说和用户无关，而和用户权限相关，这将在客户端接收消息后通过消息类型和用户权限判断是否显示。
        /// 端口共享的问题，即同一个应用程序的多个运行实例绑定到同一个[监听]端口，当消息抵达时只有第一个启动的实例能够接收到，
        /// 我们不能奢求客户端开启端口共享的服务，只能通过动态设置端口来解决.
        /// </remarks>
        /// </summary>
        [DataMember]
        public string NetPointAddress { get; set; }

        public string UDPIMIPPort
        {
            get
            {
                if (string.IsNullOrEmpty(NetPointAddress))
                    return "";
                else
                    return "soap.udp://" + NetPointAddress;
            }
        }

        [DataMember]
        public int IMReceiveAccess { get; set; }

        //给子类使用
        //WCF不支持继承，可以使用KnowType，此处用显式转换
        public UserPoint ConvertToBase()
        {
            return new UserPoint
            {
                OrganizationID = this.OrganizationID,
                OrganizationName = this.OrganizationName,
                UserID = this.UserID,
                UserName = this.UserName,
                NetPointAddress = this.NetPointAddress,
                UserGuid = this.UserGuid,
                IMReceiveAccess = this.IMReceiveAccess
            };
        }
    }
}
