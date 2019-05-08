using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Windows.Threading;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using UpdateOnline.Extension;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace UpdateOnline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebClient _client;
        private string _zipFileName, _mainApp, _bkZipFilePath;
        private Dispatcher _dispatcher;
        private FilesNeedUpdate _files;
        private UpdateHelper _helper;
        log4net.ILog _log = log4net.LogManager.GetLogger("root");

        public MainWindow()
        {
            InitializeComponent();
            _dispatcher = this.Dispatcher;
            _client = new WebClient();
            _client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            _client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            _client.Proxy = WebRequest.DefaultWebProxy;
            _client.Proxy.Credentials = new NetworkCredential();
        }

        public MainWindow(UpdateHelper helper)
            : this()
        {
            _mainApp = helper.SoftPath;
            _helper = helper;
            _files = helper.Files;
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingLabel.Text = "有新版本发布,正在备份当前文件,请稍候……";
                Action bkaction = () => BackUpFiles();
                bkaction.BeginInvoke(new AsyncCallback(HandleFilesToUpdate), bkaction);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception e)
        {
            _dispatcher.Invoke(new Action(() =>
           {
               tbError.Text = "系统升级出错,错误原因:" + e.Message + "\n未升级版本不能保证运行正常.";
               pnlError.Visibility = Visibility.Visible;
           }));
        }

        void Init()
        {
            pnlError.Visibility = Visibility.Collapsed;
            this.RadProgressBar1.Value = 0;
            PercentageLabel.Text = "";
            if (!string.IsNullOrEmpty(_bkZipFilePath) && File.Exists(_bkZipFilePath))
                File.Delete(_bkZipFilePath);
            _zipFileName = _bkZipFilePath = "";
        }

        private void HandleFilesToUpdate(IAsyncResult res)
        {
            Action action = new Action(() =>
            {
                try
                {
                    DeleteAbateFiles();
                    var filesNeedDownload = new FilesNeedUpdate
                    {
                        Files = _files.Files.ToList().FindAll(o => !o.IsDelete).ToArray(),
                        Directories = _files.Directories.ToList().FindAll(o => !o.IsDelete).ToArray()
                    };
                    if (!filesNeedDownload.IsEmpty)
                        StartDownload(filesNeedDownload);
                    else
                        ReStartMainApp();
                    _helper.SaveNewVersion();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            });
            action.BeginInvoke(null, null);
        }

        private bool ArrayIsEmpty(Array array)
        {
            return array == null || array.Length == 0;
        }

        private void StartDownload(FilesNeedUpdate files)
        {
            //var section = _helper.UpdateSection;
            //if (section == null || string.IsNullOrEmpty(section.SoftKey))
            //{
            //    ReStartMainApp();
            //}
            _dispatcher.Invoke(new Action(() =>
            {
                LoadingLabel.Text = "新版本文件远程压缩中……";
            }));//DispatcherPriority.SystemIdle:先绘制完界面再执行这段逻辑

            HttpClient httpClient = new HttpClient();
            MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            HttpContent content = new ObjectContent<FilesNeedUpdate>(files, jsonFormatter);
            var response = httpClient.PostAsync(ConfigurationManager.AppSettings["VersionApiRoot"] + "CompressFilesNeedUpdate", content).Result;
            if (response.IsSuccessStatusCode)
            {
                _zipFileName = response.Content.ReadAsAsync<string>().Result;
                response = httpClient.GetAsync(ConfigurationManager.AppSettings["VersionApiRoot"] + "GetFilesUpdateUrl?softKey=" + _helper.UpdateSection.SoftKey).Result;
                if (response.IsSuccessStatusCode)
                {
                    var url = response.Content.ReadAsAsync<string>().Result;
                    if (!url.EndsWith("/"))
                        url += "/";
                    url += _zipFileName;
                    _dispatcher.Invoke(new Action(() =>
                    {
                        //将压缩文件下载到临时文件夹
                        LoadingLabel.Text = "新版本文件下载中……";
                    }));
                    _log.Debug(url);
                    _client.DownloadFileAsync(new Uri(url), GetTempFolder() + "\\" + _zipFileName);
                }
                else
                {
                    throw new HttpRequestException(response.ReasonPhrase);
                }
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// 获取下载文件夹地址及解压文件存放地址
        /// 此地址默认为C:\Documents and Settings\当前用户名\Local Settings\Temp 文件夹
        /// </summary>
        private string GetTempFolder()
        {
            string folder = System.Environment.GetEnvironmentVariable("TEMP");
            return new DirectoryInfo(folder).FullName;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                this.RadProgressBar1.Value = e.ProgressPercentage;
                PercentageLabel.Text = e.ProgressPercentage.ToString() + " %";
            }));
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _dispatcher.Invoke(new Action(() =>
            {
                LoadingLabel.Text = PercentageLabel.Text = "";
                CompleteLabel.Text = "文件接收完成,正在更新……";
            }));
            HandleUploadedFiles();
        }

        private void HandleUploadedFiles()
        {
            FilesHandler.UnpackFiles(GetTempFolder() + "\\" + _zipFileName, this.GetAppRootPath());

            HttpClient httpClient = new HttpClient();
            var url = ConfigurationManager.AppSettings["VersionApiRoot"] + "DeleteCompressedFile?fileName="+_zipFileName;
            var response = httpClient.DeleteAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                ReStartMainApp();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
            // _dispatcher.Invoke(new Action(() =>//_dispatcher.Invoke同步到UI线程(若父方法是Dispatcher.BeginInvoke发起则不需要同步，本身一直在UI线程,Dispatcher是伪多线程模式)
            //{
            //    CompleteLabel.Text = "正在重启应用程序,请稍候……";
            //}), DispatcherPriority.Normal);
        }

        /// <summary>
        /// 删除已过期的文件
        /// </summary>
        private void DeleteAbateFiles()
        {
            var filesNeedDelete = new FilesNeedUpdate
            {
                Files = _files.Files.ToList().FindAll(o => o.IsDelete).ToArray(),
                Directories = _files.Directories.ToList().FindAll(o => o.IsDelete).ToArray()
            };
            if (!filesNeedDelete.IsEmpty)
            {
                _dispatcher.Invoke(new Action(() =>
                {
                    CompleteLabel.Text = "正在删除已过期文件……";
                }), DispatcherPriority.Normal);
                FilesHandler.DeleteFiles(filesNeedDelete.Files.Select(o => o.Name).ToArray(), filesNeedDelete.Directories.Select(o => o.Name).ToArray(), this.GetAppRootPath());
            }

        }

        private void ReStartMainApp(IAsyncResult res = null)
        {
            _dispatcher.Invoke(new Action(() =>
                {
                    CompleteLabel.Text = "正在重启应用程序,请稍候……";
                }));

            _dispatcher.BeginInvoke(new Action(() =>
                {
                    Process.Start(_mainApp, "IsUpdated");
                    this.Init();
                    Process.GetCurrentProcess().Kill();
                }), DispatcherPriority.SystemIdle);
        }

        private string GetAppRootPath()
        {
            var rootPath = System.IO.Path.GetDirectoryName(_mainApp);
            if (!rootPath.EndsWith("\\"))
                rootPath += "\\";
            return rootPath;
        }

        //更新前备份文件
        private void BackUpFiles()
        {
            var rootPath = GetAppRootPath();
            _bkZipFilePath = rootPath + Guid.NewGuid().ToString() + ".zip";
            FilesHandler.CompressFiles(_files, rootPath, _bkZipFilePath);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Init();
            Process.GetCurrentProcess().Kill();
        }

        private void btnRunApp_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_bkZipFilePath) && File.Exists(_bkZipFilePath))
                FilesHandler.UnpackFiles(_bkZipFilePath, this.GetAppRootPath());
            ReStartMainApp();
        }

        private void btnReUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.Init();
            this.MainWindow_Loaded(null, null);
        }
    }
}
