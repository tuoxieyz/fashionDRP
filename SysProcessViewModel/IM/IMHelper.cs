using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;
using System.ServiceModel;
using IWCFServiceForIM;
using DomainLogicEncap;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Collections;
using System.Net;
using UdpTransportForWCF;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace SysProcessViewModel
{
    public static class IMHelper //: IDisposable
    {
        private static List<ClientUserPoint> _localUsers;
        /// <summary>
        /// 本地存储所有用户（包括在线和已下线的，使用Guid标示，主要为了避免重复打洞）
        /// </summary>
        public static List<ClientUserPoint> LocalUsers
        {
            get
            {
                if (_localUsers == null)
                    _localUsers = GetOnlineUsers().Select(o => new ClientUserPoint(o) { IsOnline = true }).ToList();
                return _localUsers;
            }
        }
        /// <summary>
        /// 当前在线用户集合
        /// </summary>
        public static List<ClientUserPoint> OnlineUsers
        {
            get
            {
                return LocalUsers.FindAll(o => o.IsOnline);
            }
        }

        public static ObservableCollection<ClientIMessage> Messages = new ObservableCollection<ClientIMessage>();

        public static UserPoint CurrentUser { get; private set; }

        static string _serverUdpUri, _serverTcpUri;

        static SampleProfileUdpBinding _udpBinding;
        public static SampleProfileUdpBinding UdpBinding
        {
            get
            {
                if (_udpBinding == null)
                {
                    _udpBinding = new SampleProfileUdpBinding() { SendPort = GetFirstAvailablePort(), ReliableSessionEnabled = false };
                }
                return _udpBinding;
            }
        }

        static ServiceHost _service;
        static DispatcherTimer _timer = new DispatcherTimer();
        //ServiceHost _duplexService;

        static IMHelper()
        {
            _serverUdpUri = System.Configuration.ConfigurationManager.AppSettings["UdpIMServiceUri"];
            _serverTcpUri = System.Configuration.ConfigurationManager.AppSettings["TcpIMServiceUri"];
            _timer.Interval = new TimeSpan(0, 1, 0);//间隔1分钟
            _timer.Tick += delegate
            {
                InvokeServerService(service => service.HoldMyPort(CurrentUser));
            };
        }

        private static void StartService()
        {
            if (_service == null)
            {
                _service = new ServiceHost(typeof(ClientService));
                _service.AddServiceEndpoint(typeof(IClientService), UdpBinding, "soap.udp://localhost:" + UdpBinding.SendPort.ToString() + "/Client");
            }
            if (_service.State != CommunicationState.Opened)
                _service.Open();
            _timer.Start();
        }

        public static void CloseService()
        {
            if (_service != null && _service.State != CommunicationState.Closing)
            {
                _service.Close();
            }
            _timer.Stop();
        }

        public static void RegisterToIM()
        {
            UserPoint tempUser = new UserPoint
            {
                UserID = VMGlobal.CurrentUser.ID,
                UserName = VMGlobal.CurrentUser.Name,
                OrganizationID = VMGlobal.CurrentUser.OrganizationID,
                OrganizationName = OrganizationListVM.CurrentOrganization.Name,
                IMReceiveAccess = (int)VMGlobal.CurrentUser.IMReceiveAccess
            };
            if (CurrentUser != null)//并非初次登录(注销状态)
            {
                tempUser.UserGuid = CurrentUser.UserGuid;
                CurrentUser = tempUser;
            }
            else
            {
                tempUser.UserGuid = Guid.NewGuid().ToString();
                CurrentUser = tempUser;
                StartService();
            }
            InvokeServerService(service => service.UserLogin(CurrentUser));
        }

        public static void LogoutFromIM()
        {            
            if (VMGlobal.CurrentUser != null)
            {
                CurrentUser.UserID = default(int);
                InvokeServerService(service => service.UserLogout(CurrentUser));
            }
            //_udpBinding = null;
            _localUsers = null;
            Messages.Clear();
        }

        private static List<UserPoint> GetOnlineUsers()
        {
            try
            {
                using (ChannelFactory<IDuplexServerService> channelFactory = new ChannelFactory<IDuplexServerService>("TCPIMSVC", new EndpointAddress(_serverTcpUri)))
                {
                    IDuplexServerService service = channelFactory.CreateChannel();
                    var users = service.GetOnlineUsers().ToList();
                    users.RemoveAll(o => o.UserID == VMGlobal.CurrentUser.ID);
                    return users;
                }
            }
            catch//当IM服务端未启动时将抛出异常(错误，不会抛出异常)，暂不处理
            {
                return new List<UserPoint>();
            }
        }

        private static void SendMessageTo(ClientUserPoint user, IMessage message)
        {
            Action invokeAction = () =>
            {
                InvokeClientService(user, service => service.NotifyMessage(message));
            };
            if (user.IsTrustMe)
            {
                invokeAction();
            }
            else
            {
                //if (IsInnerIP(user.NetPointAddress.Split(':')[0]))
                //{
                //    user.IsTrustMe = true;
                //    invokeAction();
                //}
                //else
                //{
                Action action = () =>
                {
                    int maxTryCount = 3;//最大尝试次数
                    for (int i = 0; i < maxTryCount && !user.IsTrustMe; i++)
                    {
                        InvokeClientService(user, service => service.SayHi(CurrentUser));//我先打招呼
                        InvokeServerService(service => service.CallUserToPunchHole(user.ConvertToBase(), CurrentUser.UserGuid));//服务器叫对方给我打招呼
                        Thread.Sleep(500);
                    }
                    if (user.IsTrustMe)
                    {
                        invokeAction();
                    }
                };
                action.BeginInvoke(null, null);
                //}
            }
        }

        private static void SendMessageTo(IEnumerable<ClientUserPoint> users, IMessage message)
        {
            //虽然Parallel.ForEach使得集合操作并行执行，不过它代表的整个代码块对于它所在的线程来说仍然是同步的。
            //Parallel.ForEach(users, user =>
            //{
            //    SendMessageTo(user, message);
            //});
            foreach (var user in users)
            {
                SendMessageTo(user, message);
            }
        }

        public static void AsyncSendMessageTo(IEnumerable<ClientUserPoint> users, IMessage message,IMReceiveAccessEnum access)
        {
            Action action = () => {
                if (message.Sender == null)
                    message.Sender = IMHelper.CurrentUser;
                IEnumerable<ClientUserPoint> aims = users.Where(o => (access & (IMReceiveAccessEnum)o.IMReceiveAccess) == access);
                SendMessageTo(aims, message);
            };
            action.BeginInvoke(null, null);
        }

        internal static void InvokeServerService(Action<IServerService> action)
        {
            try
            {
                using (ChannelFactory<IServerService> channelFactory = new ChannelFactory<IServerService>(UdpBinding, new EndpointAddress(_serverUdpUri)))
                {
                    IServerService service = channelFactory.CreateChannel();
                    action(service);
                }
            }
            catch//异常暂不处理
            { }
        }

        internal static void InvokeClientService(UserPoint user, Action<IClientService> action)
        {
            try
            {
                using (ChannelFactory<IClientService> channelFactory = new ChannelFactory<IClientService>(UdpBinding, new EndpointAddress(user.UDPIMIPPort + "/Client")))
                {
                    IClientService service = channelFactory.CreateChannel();
                    action(service);
                }
            }
            catch//异常暂不处理
            { }
        }

        #region 获取可用端口供Socket通信

        /// <summary>
        /// 获取第一个可用(没被监听)的端口号
        /// </summary>
        /// <returns></returns>
        public static int GetFirstAvailablePort()
        {
            int MAX_PORT = 65535; //系统tcp/udp端口数最大是65535            
            int BEGIN_PORT = 5000;//从这个端口开始检测

            for (int i = BEGIN_PORT; i < MAX_PORT; i++)
            {
                if (PortIsAvailable(i)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 获取操作系统已用的端口号
        /// </summary>
        /// <returns></returns>
        public static IList PortIsUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序
            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序
            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            IList allPorts = new ArrayList();
            foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
            foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
            foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);

            return allPorts;
        }

        /// <summary>
        /// 检查指定端口是否已用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsAvailable(int port)
        {
            bool isAvailable = true;

            IList portUsed = PortIsUsed();

            foreach (int p in portUsed)
            {
                if (p == port)
                {
                    isAvailable = false; break;
                }
            }

            return isAvailable;
        }

        #endregion

        
    }
}
