using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace UpdateOnline
{
    public class UpdateOnlineSection : ConfigurationSection
    {
        [ConfigurationProperty("CustomerKey", DefaultValue = "")]
        public string CustomerKey
        {
            get { return (string)base["CustomerKey"]; }
            set { base["CustomerKey"] = value; }
        }

        [ConfigurationProperty("SoftKey", DefaultValue = "")]
        public string SoftKey
        {
            get { return (string)base["SoftKey"]; }
            set { base["SoftKey"] = value; }
        }

        [ConfigurationProperty("Version", DefaultValue = "")]
        public string Version
        {
            get { return (string)base["Version"]; }
            set { base["Version"] = value; }
        }
    }
}
