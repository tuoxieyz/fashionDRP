using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using IWCFServiceForIM;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using System.ServiceModel;
using System.Threading;

namespace IMServer
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public static ObservableCollection<UserPoint> OnlineUsers = new ObservableCollection<UserPoint>();

        //刷新系统运行时长和用户在线时长用到的计时器
        DispatcherTimer _timer = new DispatcherTimer();

        ServiceHost _udpService;
        ServiceHost _tcpService;

        /// <summary>
        /// 在线用户数
        /// </summary>
        public int OnlineUserCount
        {
            get { return OnlineUsers.Count; }
        }

        /// <summary>
        /// 用户登入登出记录
        /// </summary>
        public ObservableCollection<string> UserIOs { get; private set; }

        /// <summary>
        /// 服务启动时间
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// 运行时长
        /// </summary>
        public TimeSpan RunDuration
        {
            get { return DateTime.Now - StartTime; }
        }

        public MainWindowVM()
        {
            UserIOs = new ObservableCollection<string>();
            StartTime = DateTime.Now;
            OnlineUsers.CollectionChanged += new NotifyCollectionChangedEventHandler(OnlineUsers_CollectionChanged);
            _timer.Interval = new TimeSpan(0, 1, 0);//间隔1分钟
            _timer.Tick += new EventHandler(_timer_Tick);
            //Thread thread = new Thread(new ThreadStart(this.StartService)) { IsBackground = true };
            //thread.Start();//新线程开启会导致控件跨线程访问的老问题，麻烦，暂时不用这种方法。
            StartService();
        }

        private void StartService()
        {
            _udpService = new ServiceHost(typeof(ServerService));
            _tcpService = new ServiceHost(typeof(DuplexServerService));
            _udpService.Open();
            _tcpService.Open();
            StartTime = DateTime.Now;
            _timer.Start();
        }

        internal void CloseService()
        {
            if (_udpService != null && _udpService.State != CommunicationState.Closing)
            {
                _udpService.Close();
            }
            if (_tcpService != null && _tcpService.State != CommunicationState.Closing)
            {
                _tcpService.Close();
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged("RunDuration");
            foreach (var user in OnlineUsers)
            {
                var userbo = (ServerUserPoint)user;
                userbo.OnPropertyChanged("OnlineDuration");
            }
        }

        void OnlineUsers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //暂不按时间顺序推入消息
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var userbo = (ServerUserPoint)item;
                    UserIOs.Insert(0, string.Format("{0} 在 {1} 登入IM服务端", userbo.UserName, userbo.LoginTime.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var userbo = (ServerUserPoint)item;
                    UserIOs.Insert(0, string.Format("{0} 在 {1} 退出IM服务端", userbo.UserName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
            while (UserIOs.Count > 1000)//只显示最近1000条
            {
                UserIOs.RemoveAt(1000);
            }

            OnPropertyChanged("OnlineUserCount");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
