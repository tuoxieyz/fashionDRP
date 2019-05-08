using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFServiceForIM;
using System.ComponentModel;

namespace IMServer
{
    public class ServerUserPoint : UserPoint, INotifyPropertyChanged
    {
        public DateTime LoginTime { get; set; }
        public TimeSpan OnlineDuration
        {
            get
            {
                return DateTime.Now - LoginTime;
            }
        }

        public ServerUserPoint() { }

        public ServerUserPoint(UserPoint user)
        {
            this.OrganizationID = user.OrganizationID;
            this.OrganizationName = user.OrganizationName;
            this.UserID = user.UserID;
            this.UserName = user.UserName;
            this.NetPointAddress = user.NetPointAddress;
            this.UserGuid = user.UserGuid;
            this.IMReceiveAccess = user.IMReceiveAccess;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
