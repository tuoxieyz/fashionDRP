using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFServiceForIM;
using System.ComponentModel;

namespace SysProcessViewModel
{
    public class ClientUserPoint : UserPoint
    {
        /// <summary>
        /// 是否信任“我”，若信任则可直接发送消息，否则要“打洞”
        /// </summary>
        public bool IsTrustMe { get; set; }

        private bool _isOnline = true;
        /// <summary>
        /// 是否在线
        /// </summary>
        //[DefaultValue(true)]//DefaultValue这似乎只在无参构造器时有用，其它构造器构造出的实例，默认值仍为false
        public bool IsOnline { get { return _isOnline; } set { _isOnline = value; } }

        public ClientUserPoint() { }

        public ClientUserPoint(UserPoint user)
        {
            this.OrganizationID = user.OrganizationID;
            this.OrganizationName = user.OrganizationName;
            this.UserID = user.UserID;
            this.UserName = user.UserName;
            this.NetPointAddress = user.NetPointAddress;
            this.UserGuid = user.UserGuid;
            this.IMReceiveAccess = user.IMReceiveAccess;
        }
    }
}
