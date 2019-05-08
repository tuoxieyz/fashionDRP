using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace IWCFServiceForIM
{
    /// <summary>
    /// 客户端服务，主要用来接收各种消息
    /// </summary>
    [ServiceContract(Namespace = "http://www.tuoxie.com/erp/")]
    public interface IClientService
    {
        /// <summary>
        /// 用户上线通知
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyWhenUserLogin(UserPoint user);

        /// <summary>
        /// 用户下线通知
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyWhenUserLogout(UserPoint user);

        /// <summary>
        /// 消息通知
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyMessage(IMessage message);

        /// <summary>
        /// 打洞
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyPunchHole(UserPoint waitingUser);

        /// <summary>
        /// sbody say "hi" to me
        /// <remarks>属于打洞过程</remarks>
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void SayHi(UserPoint callingUser);

        /// <summary>
        /// 踢我下线
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void KickOff(UserPoint user);
    }
}
