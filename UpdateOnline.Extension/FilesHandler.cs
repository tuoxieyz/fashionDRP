using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace UpdateOnline.Extension
{
    public static class FilesHandler
    {
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="files">待压缩文件名集合</param>
        /// <param name="rootPath">待压缩文件根路径</param>
        /// <param name="zipFilePath">最后生成的压缩文件路径</param>
        public static void CompressFiles(FilesNeedUpdate files,string rootPath,string zipFilePath)
        {
            List<string> filesTemp = new List<string>();
            files.Files.ToList().ForEach(f =>
            {
                filesTemp.Add(rootPath + f);
            });
            files.Directories.ToList().ForEach(d =>
            {
                filesTemp.AddRange(FilesHandler.GetAllFilesOfDirectory(rootPath + d));
            });
            var filesDistinct = filesTemp.Distinct();
            FilesHandler.ZipFiles(filesDistinct, rootPath, zipFilePath);
            //ZipFiles(filesDistinct, zipFileName);
        }

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="filePaths">待压缩文件路径集合</param>
        /// <param name="rootPath">待压缩文件根路径</param>
        /// <param name="zipFilePath">最后生成的压缩文件路径</param>
        public static void ZipFiles(IEnumerable<string> filePaths,string rootPath, string zipFilePath)
        {
            Crc32 crc = new Crc32();
            ZipOutputStream outPutStream = new ZipOutputStream(File.Create(zipFilePath));
            outPutStream.SetLevel(9);
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    FileStream fileStream = File.OpenRead(filePath);
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    ZipEntry entry = new ZipEntry(filePath.Replace(rootPath, string.Empty));
                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    outPutStream.PutNextEntry(entry);
                    outPutStream.Write(buffer, 0, buffer.Length);
                }
            }
            outPutStream.Finish();
            outPutStream.Close();
        }

        /// <summary>
        /// 获取文件夹下的所有文件（包括子文件夹下的文件）
        /// </summary>
        public static List<string> GetAllFilesOfDirectory(string dirName)
        {
            List<string> files = new List<string>();
            string[] subPaths = Directory.GetDirectories(dirName);

            foreach (string path in subPaths)
            {
                files.AddRange(GetAllFilesOfDirectory(path));
            }
            files.AddRange(Directory.GetFiles(dirName));
            return files;
        }

        /// <summary>
        /// 解压文件到指定文件夹
        /// </summary>
        public static void UnpackFiles(string file, string dir)
        {
            if (!dir.EndsWith("\\"))
                dir += "\\";            
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            ZipInputStream s = new ZipInputStream(File.OpenRead(file));
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);
                if (directoryName != String.Empty)
                    Directory.CreateDirectory(dir + directoryName);
                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(dir + theEntry.Name);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0) { streamWriter.Write(data, 0, size); }
                        else { break; }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
        }

        /// <summary>
        /// 删除文件[夹]
        /// </summary>
        public static void DeleteFiles(string[] files, string[] dirs, string baseDir)
        {
            if (!baseDir.EndsWith("\\"))
                baseDir += "\\";
            for (int i = 0; i < files.Length; i++)
            {
                var file = baseDir + files[i];
                if (File.Exists(file))
                    File.Delete(file);
            }
            for (int i = 0; i < dirs.Length; i++)
            {
                var dir = baseDir + dirs[i];
                if (Directory.Exists(dir))
                    Directory.Delete(dir);
            }
        }
    }
}
