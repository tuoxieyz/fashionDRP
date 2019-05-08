using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFServiceForIM;
using System.Windows.Media;

namespace SysProcessViewModel
{
    public class ClientIMessage : IMessage
    {
        public Brush MessageColor
        {
            get
            {
                if (MessageKind == 0)
                {
                    return Brushes.DarkBlue;
                }
                else
                {
                    return Brushes.DarkRed;
                }
            }
        }

        public ClientIMessage() { }

        public ClientIMessage(IMessage message)
        {
            this.Message = message.Message;
            this.Sender = message.Sender;
            this.SendTime = message.SendTime;
            this.MessageKind = message.MessageKind;
        }
    }
}
