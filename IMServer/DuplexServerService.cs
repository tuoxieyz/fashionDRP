using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFServiceForIM;

namespace IMServer
{
    public class DuplexServerService : IDuplexServerService
    {
        public UserPoint[] GetOnlineUsers()
        {
            //OperationContext context = OperationContext.Current;
            //MessageProperties properties = context.IncomingMessageProperties;
            //IPEndPoint endpoint = properties[RemoteEndpointMessageProperty.Name] as IPEndPoint;
            //UserPoint user = new UserPoint { NetPoint = endpoint };
            //context.OutgoingMessageProperties.Add("via", user.UDPIMIPPort + "/Client");
            return MainWindowVM.OnlineUsers.Select(o => o.ConvertToBase()).ToArray();
        }
    }
}
