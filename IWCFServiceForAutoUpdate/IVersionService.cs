using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;
using UpdateOnline.Extension;

namespace IWCFServiceForAutoUpdate
{
    [ServiceContract(Namespace = "http://www.tuoxie.com/version/")]
    public interface IVersionService
    {
        [OperationContract]
        DataSet GetFilesNeedUpdate(string customerKey, string softKey, string nowVersion);

        [OperationContract]
        string GetFilesUpdateUrl(string softKey);

        [OperationContract]
        string CompressFilesNeedUpdate(FilesNeedUpdate files);

        /// <summary>
        /// 下载成功后删除压缩文件
        /// </summary>
        /// <param name="fileName">压缩文件名</param>
        [OperationContract]
        void DeleteCompressedFile(string fileName);
    }
}
