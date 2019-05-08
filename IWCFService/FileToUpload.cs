using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;

namespace IWCFService
{
    [MessageContract]
    public class FileToUpload
    {
        [MessageHeader]
        public string FileName{ get; set; }
        [MessageBodyMember]
        public Stream FileContent{ get; set; }
    }

    [MessageContract]
    public class BoolMessage
    {
        [MessageBodyMember]
        public bool Flag{ get; set; }
    }
}
