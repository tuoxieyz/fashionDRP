using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.ServiceModel.Web;

namespace IWCFService
{
    [ServiceContract(Namespace = "http://www.tuoxie.com/upload/")]
    public interface IUploadService
    {
        [OperationContract]
        BoolMessage SaveUploadedFile(FileToUpload file);
    }
}
