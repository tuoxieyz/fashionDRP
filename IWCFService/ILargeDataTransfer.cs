using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace IWCFService
{
    /// <summary>
    /// 大数据传输接口
    /// </summary>
    [ServiceContract]
    public interface ILargeDataTransfer
    {
        [OperationContract]
        byte[] GetBytes(int intStep);

        [OperationContract]
        int GetTimes();
    }
}
