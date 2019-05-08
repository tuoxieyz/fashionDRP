using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace IWCFService
{
    [ServiceContract(Namespace = "http://www.tuoxie.com/upload/", CallbackContract = typeof(ILargeDataTransfer))]
    public interface IStylePictureUploadService
    {
        [OperationContract]
        bool UploadPicture(int brandID, int year, int quarter, string fileName);
    }
}
