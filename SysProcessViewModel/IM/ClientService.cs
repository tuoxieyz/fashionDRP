using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFServiceForIM;
using System.ServiceModel;

namespace SysProcessViewModel
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class ClientService : IClientService
    {
        public void NotifyWhenUserLogin(UserPoint user)
        {
            //添加或更新在线用户信息
            if (VMGlobal.CurrentUser != null && user.UserID != VMGlobal.CurrentUser.ID)//过滤掉自己的登录消息
            {
                var localUser = IMHelper.LocalUsers.Find(o => o.UserGuid == user.UserGuid);
                if (localUser == null)
                    IMHelper.LocalUsers.Add(new ClientUserPoint(user));
                else
                {
                    //int index = IMHelper.OnlineUsers.IndexOf(localUser);
                    //IMHelper.OnlineUsers[index] = new ClientUserPoint(user);
                    localUser.OrganizationID = user.OrganizationID;
                    localUser.OrganizationName = user.OrganizationName;
                    localUser.UserID = user.UserID;
                    localUser.UserName = user.UserName;
                    localUser.NetPointAddress = user.NetPointAddress;//既然Guid一样，这个应该也是一样的
                    localUser.IsOnline = true;
                    localUser.IMReceiveAccess = user.IMReceiveAccess;
                }
            }
        }

        public void NotifyWhenUserLogout(UserPoint user)
        {
            if (VMGlobal.CurrentUser != null)//判断是否已注销，即退出的是否自己
            {
                var localUser = IMHelper.OnlineUsers.Find(o => o.UserGuid == user.UserGuid);
                if (localUser != null)
                    localUser.IsOnline = false;
            }
        }

        public void NotifyMessage(IMessage message)
        {
            var localUser = IMHelper.LocalUsers.Find(o => o.UserGuid == message.Sender.UserGuid && !o.IsTrustMe);
            //打洞成功后可能会产生A将B加入信任列表，但B没有将A加入信任列表的情况，原因是A给B第一次发送的数据包（SayHi方法）会被B端NAT丢弃
            //若A没有在B发给A SayHi 消息后再次 SayHi to B，那么B将不把A列为信任对象即可通信对象（因为它不知道A是否接收到了它的请求）
            //于是我们只能在业务消息抵达时再设置IsTrustMe = true
            if (localUser != null)
                localUser.IsTrustMe = true;
            IMHelper.Messages.Insert(0, new ClientIMessage(message));
        }

        public void NotifyPunchHole(UserPoint waitingUser)
        {
            using (ChannelFactory<IClientService> channelFactory = new ChannelFactory<IClientService>(IMHelper.UdpBinding, new EndpointAddress(waitingUser.UDPIMIPPort + "/Client")))
            {
                IClientService service = channelFactory.CreateChannel();
                service.SayHi(IMHelper.CurrentUser);
            }
        }

        //调试时发现该服务操作执行在主线程，会否是因为主线程空闲所以WCF没有额外分配新的线程呢？
        public void SayHi(UserPoint callingUser)
        {
            if (VMGlobal.CurrentUser != null)
            {
                var user = IMHelper.OnlineUsers.Find(o => o.UserGuid == callingUser.UserGuid);
                if (user != null)
                    user.IsTrustMe = true;
            }
        }

        public void KickOff(UserPoint user)
        {
            if (user.UserGuid == IMHelper.CurrentUser.UserGuid && VMGlobal.CurrentUser != null)
            {
                MainWindowVM.Current.Logout(true);
            }
        }
    }
}
