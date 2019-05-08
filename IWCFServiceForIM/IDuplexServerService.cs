using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace IWCFServiceForIM
{
    [ServiceContract(Namespace = "http://www.tuoxie.com/erp/")]
    public interface IDuplexServerService
    {
        /// <summary>
        /// 获取当前在线的用户集合
        /// </summary>
        [OperationContract]
        UserPoint[] GetOnlineUsers();
    }
}
