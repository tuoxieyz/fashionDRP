using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using IWCFService;
using System.IO;

namespace WCFServiceHost
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UploadService" in code, svc and config file together.
    public class UploadService : IUploadService
    {
        public BoolMessage SaveUploadedFile(FileToUpload file)
        {
            var filePath = System.Web.HttpRuntime.AppDomainAppPath.ToString() + file.FileName;
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            FileStream f = new FileStream(filePath, FileMode.Create);
            try
            {
                BinaryWriter writer = new BinaryWriter(f);
                BinaryReader reader = new BinaryReader(file.FileContent);
                byte[] buffer;
                do
                {
                    buffer = reader.ReadBytes(20480);
                    writer.Write(buffer);
                } while (buffer.Length > 0);
                return new BoolMessage { Flag = true };
            }
            catch
            {
                return new BoolMessage { Flag = false };
            }
            finally
            {
                f.Close();
                file.FileContent.Close();
            }
        }
    }
}
