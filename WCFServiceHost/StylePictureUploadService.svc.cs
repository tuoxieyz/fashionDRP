using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using IWCFService;
using System.IO;
using Kernel;
using System.Drawing;

namespace WCFServiceHost
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StylePictureUploadService : IStylePictureUploadService
    {
        public bool UploadPicture(int brandID, int year, int quarter, string fileName)
        {
            ILargeDataTransfer callback = OperationContext.Current.GetCallbackChannel<ILargeDataTransfer>();

            int intNum = callback.GetTimes(); //获取读取字节流的次数
            MemoryStream mstream = new MemoryStream();
            byte[] byteATime;
            for (int i = 0; i < intNum; i++)
            {
                byteATime = callback.GetBytes(i);
                mstream.Write(byteATime, 0, byteATime.Length);//写到内存中
            }
            mstream.Position = 0;
            byteATime = new byte[mstream.Length];
            mstream.Read(byteATime, 0, byteATime.Length);//从内存中读到getbyte中
            mstream.Close();

            //反序列化
            Image data = (Image)DataExtension.RetrieveDataDecompress(byteATime);
            var fileSavePath = System.Web.HttpRuntime.AppDomainAppPath.ToString() + "StylePicture\\";
            fileSavePath += brandID.ToString("00") + "\\" + year + quarter.ToString("00") + "\\";
            if (!Directory.Exists(fileSavePath))
                Directory.CreateDirectory(fileSavePath);
            try
            {
                data.Save(fileSavePath + fileName);
                ImageHandler.ToThumbnail(fileSavePath + fileName, 200, 300);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                data.Dispose();
            }
        }
    }
}
