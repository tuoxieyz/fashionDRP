using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using IWCFService;
using System.Configuration;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.ServiceModel.Activation;
using DBAccess;
using CentralizeModel;
using UpdateOnline.Extension;
using IWCFServiceForAutoUpdate;

namespace WCFServiceHost
{
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]//这样就可以使用Session了
    public class VersionService : IVersionService
    {
        private static QueryGlobal _query = new QueryGlobal("PlatformCentralizeConnection");

        private static Database DB { get { return _query.DB; } }

        public DataSet GetFilesNeedUpdate(string customerKey, string softKey, string nowVersion)
        {
            var ds = DB.ExecuteDataSet("GetFilesNeedUpdate", customerKey, softKey, nowVersion);
            return ds;
        }

        public string GetFilesUpdateUrl(string softKey)
        {
            return _query.LinqOP.Search<SoftToUpdate>(o => o.IdentificationKey == softKey).Select(o => o.UpdateUrl).FirstOrDefault();
        }

        public string CompressFilesNeedUpdate(FilesNeedUpdate files)
        {
            var rootPath = GetVersionFilesRootPath();
            //List<string> filesTemp = new List<string>();
            //files.Files.ToList().ForEach(f =>
            //{
            //    filesTemp.Add(rootPath + f);
            //});
            //files.Directories.ToList().ForEach(d =>
            //{
            //    filesTemp.AddRange(FilesHandler.GetAllFilesFromDirectory(rootPath + d));
            //});
            //var filesDistinct = filesTemp.Distinct();
            var zipFileName = Guid.NewGuid().ToString() + ".zip";
            //FilesHandler.ZipFiles(filesDistinct, rootPath, System.Web.HttpRuntime.AppDomainAppPath.ToString() + "VersionFile\\" + zipFileName);
            ////ZipFiles(filesDistinct, zipFileName);
            //return zipFileName;
            FilesHandler.CompressFiles(files, rootPath, System.Web.HttpRuntime.AppDomainAppPath.ToString() + "VersionFile\\" + zipFileName);
            return zipFileName;
        }

        private string GetVersionFilesRootPath()
        {
            var rootPath = ConfigurationManager.AppSettings["VersionRootPath"];
            if (!rootPath.EndsWith("\\"))
                rootPath += "\\";
            return rootPath;
        }

        //private void ZipFiles(IEnumerable<string> files, string zipFileName)
        //{
        //    var rootPath = GetVersionFilesRootPath();
        //    Crc32 crc = new Crc32();
        //    ZipOutputStream outPutStream = new ZipOutputStream(File.Create());
        //    outPutStream.SetLevel(9);
        //    foreach (string file in files)
        //    {
        //        FileStream fileStream = File.OpenRead(file);
        //        byte[] buffer = new byte[fileStream.Length];
        //        fileStream.Read(buffer, 0, buffer.Length);
        //        ZipEntry entry = new ZipEntry(file.Replace(rootPath, string.Empty));
        //        entry.DateTime = DateTime.Now;
        //        entry.Size = fileStream.Length;
        //        fileStream.Close();
        //        crc.Reset();
        //        crc.Update(buffer);
        //        entry.Crc = crc.Value;
        //        outPutStream.PutNextEntry(entry);
        //        outPutStream.Write(buffer, 0, buffer.Length);
        //    }
        //    outPutStream.Finish();
        //    outPutStream.Close();
        //}

        /// <summary>
        /// 获取文件夹下的所有文件（包括子文件夹下的文件）
        /// </summary>
        //private List<string> GetAllFilesFromDirectory(string dirName)
        //{
        //    List<string> files = new List<string>();
        //    string[] subPaths = Directory.GetDirectories(dirName);

        //    foreach (string path in subPaths)
        //    {
        //        files.AddRange(GetAllFilesFromDirectory(path));
        //    }
        //    files.AddRange(Directory.GetFiles(dirName));
        //    return files;
        //}

        public void DeleteCompressedFile(string fileName)
        {
            var filePath = System.Web.HttpRuntime.AppDomainAppPath.ToString() + "VersionFile\\" + fileName;
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
