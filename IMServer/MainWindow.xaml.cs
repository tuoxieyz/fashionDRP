using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IWCFServiceForIM;
using System.Collections;

namespace IMServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var dataContext = new MainWindowVM();
            this.DataContext = dataContext;
            InitializeComponent();
            this.Closing += delegate
            {
                dataContext.CloseService();
            };
        }

        private void kickoff_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            var user = img.DataContext as UserPoint;
            lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)
            {
                MainWindowVM.OnlineUsers.Remove(user);
            }
            ServerService.InvokeClientService(user, service => service.KickOff(user.ConvertToBase()));
        }

        private void broadcast_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbBroadcastMessage.Text))
            {
                lock (((ICollection)MainWindowVM.OnlineUsers).SyncRoot)
                {
                    for (int i = 0; i < MainWindowVM.OnlineUsers.Count; i++)
                    {
                        var u = MainWindowVM.OnlineUsers.ElementAtOrDefault(i);
                        if (u != null)
                            ServerService.InvokeClientService(u, service => service.NotifyMessage(new IMessage
                            {
                                Message = tbBroadcastMessage.Text,
                                MessageKind = 1,
                                Sender = new UserPoint { UserName = "系统管理员", OrganizationName = "系统控制中心", UserGuid = Guid.Empty.ToString() },
                                SendTime = DateTime.Now
                            }));
                    }
                }
                tbBroadcastMessage.Clear();
            }
        }
    }
}
