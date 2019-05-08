using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;
using System.ServiceModel;

namespace IWCFServiceForIM
{
    [ServiceContract(Namespace = "http://www.tuoxie.com/erp/")]
    public interface IServerService
    {
        /// <summary>
        /// 用户登入[到服务器端用户列表]
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void UserLogin(UserPoint user);

        /// <summary>
        /// 用户登出[移出服务器端用户列表]
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void UserLogout(UserPoint user);

        /// <summary>
        /// 叫用户A给用户B方向发一条消息（打洞）
        /// </summary>
        /// <param name="callingUser">打洞方</param>
        /// <param name="waitingUserID">等待方标识</param>
        [OperationContract(IsOneWay = true)]
        void CallUserToPunchHole(UserPoint callingUser, string waitingUserGuid);

        /// <summary>
        /// 维持映射端口
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void HoldMyPort(UserPoint user);
    }
}
