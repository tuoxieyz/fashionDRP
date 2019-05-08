//*********************************************
// 公司名称：              
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-6-15 14:05:53
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using System.Configuration;
using UpdateOnline.Extension;
using System.Net.Http;
using CentralizeModel;

namespace UpdateOnline
{
    public class UpdateHelper
    {
        public string SoftPath
        {
            get;
            set;
        }

        private FilesNeedUpdate _files;
        /// <summary>
        /// 待升级文件列表
        /// </summary>
        public FilesNeedUpdate Files
        {
            get
            {
                if (_files == null)
                {
                    _files = this.GetFilesNeedUpdate();
                }
                return _files;
            }
        }

        private UpdateOnlineSection _updateSection;
        public UpdateOnlineSection UpdateSection
        {
            get
            {
                if (_updateSection == null)
                {
                    var config = ConfigurationManager.OpenExeConfiguration(SoftPath);
                    _updateSection = config.Sections["UpdateOnline"] as UpdateOnlineSection;
                }
                return _updateSection;
            }
        }

        public UpdateHelper(string softPath)
        {
            SoftPath = softPath;
        }

        internal void SaveNewVersion()
        {
            UpdateSection.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// 获取需要升级的文件和目录
        /// </summary>
        /// <param name="softPath">待升级软件的路径</param>
        private FilesNeedUpdate GetFilesNeedUpdate()
        {
            if (UpdateSection == null)
                return null;

            HttpClient httpClient = new HttpClient();
            var url = ConfigurationManager.AppSettings["VersionApiRoot"] + "GetFilesNeedUpdate";
            url = string.Format("{3}?customerKey={0}&softKey={1}&nowVersion={2}", UpdateSection.CustomerKey, UpdateSection.SoftKey, UpdateSection.Version, url);
            //_log.Debug(url);
            var response = httpClient.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                var versions = response.Content.ReadAsAsync<List<SoftVersionTrack>>().Result;
                if (versions.Count == 0)
                    return null;
                if (!versions.Exists(v => v.IsCoerciveUpdate))//是否强制更新
                    return null;
                UpdateSection.Version = versions[0].VersionCode.ToString();
                List<FileNeedUpdate> files = new List<FileNeedUpdate>();
                List<FileNeedUpdate> directories = new List<FileNeedUpdate>();
                XmlDocument doc = new XmlDocument();
                foreach (SoftVersionTrack version in versions)
                {
                    doc.LoadXml(version.UpdatedFileList);
                    var tempFiles = GetNodeNameList(doc.GetElementsByTagName("file")).ToList();
                    var tempDires = GetNodeNameList(doc.GetElementsByTagName("directory")).ToList();
                    files = Coverforward(tempFiles, files);
                    directories = Coverforward(tempDires, directories);
                }
                return new FilesNeedUpdate { Files = files.ToArray(), Directories = directories.ToArray() };
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        private IEnumerable<FileNeedUpdate> GetNodeNameList(XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                var name = node.Attributes["name"].Value;
                FileNeedUpdate item = new FileNeedUpdate { Name = name };
                var dnode = node.Attributes["isDelete"];
                if (dnode != null)
                    item.IsDelete = Convert.ToBoolean(dnode.Value);
                yield return item;
            }
        }

        /// <summary>
        /// 前向覆盖
        /// </summary>
        private List<FileNeedUpdate> Coverforward(List<FileNeedUpdate> filesFormer, List<FileNeedUpdate> filesAfter)
        {
            var diff = filesFormer.Except(filesAfter);
            filesAfter.AddRange(diff);
            return filesAfter;
        }
    }
}
