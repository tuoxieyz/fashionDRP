using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFServiceForIM;
using Kernel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using System.Threading.Tasks;
using System.Collections;

namespace IMServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServerService : IServerService
    {
        public void UserLogin(UserPoint user)
        {
            lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)//估计：在lock中启用新线程，当lock中的代码执行完毕而新线程还在执行，那么新线程后续代码不再处于lock保护之下
            {
                var users = MainWindowVM.OnlineUsers.Where(o => o.UserID == user.UserID && o.UserGuid != user.UserGuid).ToArray();
                if (users.Count() > 0)
                {
                    Parallel.ForEach(users, u =>
                    {
                        MainWindowVM.OnlineUsers.Remove(u);
                        ServerService.InvokeClientService(u, service => service.KickOff(u.ConvertToBase()));
                    });
                }
            }

            //var orgiUser = MainWindowVM.OnlineUsers.FirstOrDefault(o => o.UserGuid == user.UserGuid);
            //if (orgiUser == null)
            //{
            OperationContext context = OperationContext.Current;
            //获取传进的消息属性
            MessageProperties properties = context.IncomingMessageProperties;
            //获取消息发送的远程终结点IP和端口
            IPEndPoint endpoint = properties[RemoteEndpointMessageProperty.Name] as IPEndPoint;
            //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            user.NetPointAddress = endpoint.ToString();//new IPEndPoint(IPAddress.Parse(endpoint.Address), endpoint.Port);
            //context.OutgoingMessageProperties.Add("via", user.UDPIMIPPort);
            lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)
            {
                MainWindowVM.OnlineUsers.Add(new ServerUserPoint(user) { LoginTime = DateTime.Now });
            }
            //}
            //else//若存在相同Guid用户则更新，防止客户端之间多次打洞(后注：此处不可能出现这种情况)
            //{
            //    lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)
            //    {
            //        int index = MainWindowVM.OnlineUsers.IndexOf(orgiUser);
            //        MainWindowVM.OnlineUsers[index] = new ServerUserPoint(user) { LoginTime = DateTime.Now };
            //    }
            //}

            NotifyWhenUserLogin(user);
            //return new OPResult { IsSucceed = true };
        }

        /// <summary>
        /// 通知所有在线用户有新用户上线了
        /// </summary>
        /// <param name="user">上线用户</param>
        private void NotifyWhenUserLogin(UserPoint user)
        {
            lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)//避免在循环过程中集合被修改
            {
                for (int i = 0; i < MainWindowVM.OnlineUsers.Count; i++)
                {
                    var u = MainWindowVM.OnlineUsers.ElementAtOrDefault(i);
                    if (u != null && u.UserID != user.UserID)
                        InvokeClientService(u, service => service.NotifyWhenUserLogin(user));
                }
            }
        }

        public void UserLogout(UserPoint user)
        {
            UserPoint userbo = MainWindowVM.OnlineUsers.FirstOrDefault(o => o.UserGuid == user.UserGuid);
            if (userbo != null)
            {
                lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)
                {
                    MainWindowVM.OnlineUsers.Remove(userbo);
                }
                //Action action = () => NotifyWhenUserLogout(userbo.ConvertToUser());
                //action.BeginInvoke(null, null);
                NotifyWhenUserLogout(userbo.ConvertToBase());//wcf本身就支持多线程，没必要额外开线程
            }
        }

        /// <summary>
        /// 通知所有在线用户有用户下线了
        /// </summary>
        /// <param name="user">下线用户</param>
        private void NotifyWhenUserLogout(UserPoint user)
        {
            lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)
            {
                for (int i = 0; i < MainWindowVM.OnlineUsers.Count; i++)
                {
                    var u = MainWindowVM.OnlineUsers.ElementAtOrDefault(i);
                    if (u != null && u.UserID != user.UserID)
                        InvokeClientService(u, service => service.NotifyWhenUserLogout(user));
                }
            }
        }

        public void CallUserToPunchHole(UserPoint callingUser, string waitingUserGuid)
        {
            var waitingUser = MainWindowVM.OnlineUsers.FirstOrDefault(o => o.UserGuid == waitingUserGuid);
            if (waitingUser != null)//==null的情况暂不考虑
            {
                InvokeClientService(callingUser, service => service.NotifyPunchHole(waitingUser.ConvertToBase()));
            }
        }

        internal static void InvokeClientService(UserPoint user, Action<IClientService> action)
        {
            try
            {
                //address组成部分要和服务端配置文件保持一致，"/Client"不能丢
                using (ChannelFactory<IClientService> channelFactory = new ChannelFactory<IClientService>("IMClientSVC", new EndpointAddress(user.UDPIMIPPort + "/Client")))
                {
                    IClientService service = channelFactory.CreateChannel();
                    action(service);
                }
            }
            catch//异常暂不处理
            { }
        }

        public void HoldMyPort(UserPoint user)
        {
            if (user.UserID != default(int) && !MainWindowVM.OnlineUsers.Any(o => o.UserGuid == user.UserGuid))
            {
                UserLogin(user);
            }
        }
    }
}
