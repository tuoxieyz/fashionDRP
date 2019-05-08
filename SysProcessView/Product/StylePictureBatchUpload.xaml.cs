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
using SysProcessViewModel;
using Telerik.Windows.Controls;
using System.IO;
using IWCFService;
using System.ServiceModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using SysProcessModel;
using System.Collections.ObjectModel;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for StylePictureBatchUpload.xaml
    /// </summary>
    public partial class StylePictureBatchUpload : UserControl
    {
        public StylePictureBatchUpload()
        {
            InitializeComponent();
            //cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            //cbxQuarter.ItemsSource = VMGlobal.Quarters;
        }

        //private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        DateTime date = (DateTime)e.AddedItems[0];
        //        RadDatePicker picker = sender as RadDatePicker;
        //        picker.DateTimeText = date.Year.ToString();
        //    }
        //}

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            //var byq = this.CheckSetting();
            //if (byq != null)
            //{
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "图片|*.jpg;*.png;*.gif;*.bmp;*.jpeg";
            dlg.Multiselect = true;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string[] filenames = dlg.FileNames;
                int invalidNum = filenames.Count(o => !o.Contains('-'));
                if (invalidNum > 0)
                {
                    MessageBox.Show("有" + invalidNum + "个图片文件名不符合要求.\n图片文件名需要以短横杠'-'分隔的款号色号组成.");//若有多个横杠则以最后一个为分隔符
                    return;
                }
                var pfiles = filenames.Select(o =>
                {
                    FileInfo file = new FileInfo(o);
                    var filename = System.IO.Path.GetFileNameWithoutExtension(o);
                    int index = filename.LastIndexOf('-');
                    string scode = filename.Substring(0, index);
                    string ccode = filename.Substring(index + 1);
                    return new StylePictureFile
                    {
                        StyleCode = scode,
                        ColorCode = ccode,
                        FilePath = o,
                        FileSize = file.Length / 1024.0f,
                        //SCCode = filename,
                        UploadStatus = "未上传",
                        UploadProgress = 0.0f,
                        PictureName = file.Name
                    };
                }).ToList();
                var excepts = pfiles.Select(o => o.ColorCode).Except(VMGlobal.Colors.Select(o => o.Code));
                if (excepts.Count() > 0)
                {
                    string errorinfo = "";
                    foreach (string except in excepts)
                    {
                        errorinfo += (except + ",");
                    }
                    MessageBox.Show("文件名中包含下列不存在的颜色编号:\n" + errorinfo.TrimEnd(','));
                    return;
                }
                IEnumerable<string> scodes = pfiles.Select(o => o.StyleCode);
                IEnumerable<int> bids = VMGlobal.BYQs.Select(o => o.ID);
                var tempscodes = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle>(o => scodes.Contains(o.Code) && bids.Contains(o.BYQID)).Select(o => new { o.Code, o.BYQID, o.ID }).ToList();
                excepts = scodes.Except(tempscodes.Select(o => o.Code));
                if (excepts.Count() > 0)
                {
                    string errorinfo = "";
                    foreach (string except in excepts)
                    {
                        errorinfo += (except + ",");
                    }
                    MessageBox.Show("文件名中包含下列不存在的款式编号:\n" + errorinfo.TrimEnd(','));
                    return;
                }
                pfiles.ForEach(o =>
                    {
                        var tempstyle = tempscodes.Find(s => s.Code == o.StyleCode);
                        o.BYQID = tempstyle.BYQID;
                        o.StyleID = tempstyle.ID;
                        o.ColorID = VMGlobal.Colors.Find(c => c.Code == o.ColorCode).ID;
                    });
                //List<StylePictureFile> pfiles = new List<StylePictureFile>();
                //foreach (var filename in filenames)
                //{
                //    FileInfo file = new FileInfo(filename);
                //    var pfile = new StylePictureFile
                //    {
                //        FilePath = filename,
                //        UploadStatus = "未上传",
                //        UploadProgress = 0.0f,
                //        FileName = System.IO.Path.GetFileNameWithoutExtension(filename),
                //        FileSize = file.Length / 1024.0f
                //    };
                //    pfiles.Add(pfile);
                //}
                lvPictures.ItemsSource = new ObservableCollection<StylePictureFile>(pfiles);
            }
            //}
        }

        private DispatcherOperation BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            return this.Dispatcher.BeginInvoke(action, priority);
        }

        private void btnSingleUpload_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            //var byq = this.CheckSetting();
            //if (byq != null)
            //{
            btn.IsEnabled = false;
            StylePictureFile file = (StylePictureFile)btn.DataContext;
            var tp = btn.TemplatedParent;
            var tbStatus = View.Extension.UIHelper.GetVisualChild<TextBlock>(tp, "tbStatus");
            tbStatus.Text = "上传中";
            this.BeginInvoke(() =>
            {
                var pbProgress = View.Extension.UIHelper.GetVisualChild<RadProgressBar>(tp, "pbProgress");
                UploadPictureForSingleStyle(VMGlobal.BYQs.Find(o => o.ID == file.BYQID), file, pbProgress, btn);
            }, DispatcherPriority.Background);
            //}
        }

        //private ProBYQ CheckSetting()
        //{
        //    RadComboBox cb1 = (RadComboBox)cbxBrand.Content;
        //    if (cb1.SelectedIndex == -1)
        //    {
        //        MessageBox.Show("请设置上传款式对应的品牌");
        //        cb1.Focus();
        //        return null;
        //    }
        //    RadComboBox cb2 = (RadComboBox)cbxQuarter.Content;
        //    if (cb2.SelectedIndex == -1)
        //    {
        //        MessageBox.Show("请设置上传款式对应的季度");
        //        cb2.Focus();
        //        return null;
        //    }
        //    RadDatePicker dp = (RadDatePicker)SYear.Content;
        //    if (string.IsNullOrEmpty(dp.DateTimeText))
        //    {
        //        MessageBox.Show("请设置上传款式对应的年份");
        //        dp.Focus();
        //        return null;
        //    }
        //    var byq = DomainLogicEncap.ProductLogic.GetBYQ((int)cb1.SelectedValue, dp.DateTimeText, (int)cb2.SelectedValue);
        //    if (byq == null)
        //    {
        //        MessageBox.Show("不存在对应的品牌年份季度信息");
        //        return null;
        //    }
        //    return byq;
        //}

        /// <summary>
        /// 上传款式图片
        /// </summary>
        /// <returns>是否上传成功</returns>
        private void UploadPictureForSingleStyle(ProBYQ byq, StylePictureFile file, RadProgressBar pbProgress, RadButton btn)
        {
            var filePath = file.FilePath;
            var image = System.Drawing.Image.FromFile(filePath);
            //BitmapImage image = new BitmapImage(new Uri(filePath));            
            var transfer = new LargeDataTransfer() { Data = image };
            transfer.CallbackEvent += delegate(int progress)
            {
                this.Dispatcher.Invoke(new Action(() =>
                    {
                        pbProgress.Value = progress;
                    }));
            };
            var func = new Func<ProBYQ, string, ILargeDataTransfer, bool>(UploadPictureForSingleStyle);
            AsyncCallback callback = new AsyncCallback(ar =>
            {
                //var func = (Func<ProStyleBYQ, string, ILargeDataTransfer, bool>)ar.AsyncState;
                var flag = func.EndInvoke(ar);
                var result = ProductDataContext.SaveSCPicture(file);
                if (!flag || !result.IsSucceed)
                {
                    btn.IsEnabled = true;
                    btn.Content = "重新上传";
                    file.UploadProgress = 0;
                    file.UploadStatus = "上传失败";
                    //MessageBox.Show("上传失败");
                }
                else
                {
                    //ProSCPicture scp = new ProSCPicture { SCCode = System.IO.Path.GetFileNameWithoutExtension(filePath), PictureName = System.IO.Path.GetFileName(filePath) };
                    file.UploadStatus = "上传成功";
                    //MessageBox.Show("上传成功");
                }
            });
            func.BeginInvoke(byq, filePath, transfer, callback, func);
            //IAsyncResult ar = func.BeginInvoke(byq, filePath, transfer, null, null);
            //WaitHandle waitHandle = ar.AsyncWaitHandle;
            //waitHandle.WaitOne();//主线程等待,这里将产生死锁

            //bool flag = false;//func.EndInvoke(ar); 
            //new System.Threading.Thread(() =>
            //    {
            //        UploadPictureForSingleStyle(byq, filePath);
            //    }).Start();
        }

        private bool UploadPictureForSingleStyle(ProBYQ byq, string filePath, ILargeDataTransfer transfer)
        {
            InstanceContext instanceContext = new InstanceContext(transfer);
            bool flag = false;
            using (DuplexChannelFactory<IStylePictureUploadService> channelFactory = new DuplexChannelFactory<IStylePictureUploadService>(instanceContext, "StylePictureUploadSVC"))
            {
                IStylePictureUploadService proxy = channelFactory.CreateChannel();
                flag = proxy.UploadPicture(byq.BrandID, byq.Year, byq.Quarter, System.IO.Path.GetFileName(filePath));
            }
            return flag;
        }

        #region 辅助类

        private class StylePictureFile : ProSCPicture, INotifyPropertyChanged
        {
            public string StyleCode { get; set; }
            public string ColorCode { get; set; }
            public int BYQID { get; set; }
            public string FilePath { get; set; }
            //public string FileName { get; set; }
            public float FileSize { get; set; }

            private string _uploadStatus;
            public string UploadStatus
            {
                get { return _uploadStatus; }
                set
                {
                    if (_uploadStatus != value)
                    {
                        _uploadStatus = value;
                        OnPropertyChanged("UploadStatus");
                    }
                }
            }

            private double _uploadProgress;
            public double UploadProgress
            {
                get { return _uploadProgress; }
                set
                {
                    if (_uploadProgress != value)
                    {
                        _uploadProgress = value;
                        OnPropertyChanged("UploadProgress");
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private void btnBatchUpload_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < lvPictures.Items.Count; i++)
            {
                var item = lvPictures.Items[i];
                var cp = (ContentPresenter)lvPictures.ItemContainerGenerator.ContainerFromItem(item);
                //var cp = View.Extension.UIHelper.GetVisualChild<ContentPresenter>(container);
                DataTemplate template = cp.ContentTemplate;
                RadButton btnUpload = (RadButton)template.FindName("btnSingleUpload", cp);
                this.btnSingleUpload_Click(btnUpload, null);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<StylePictureFile> pfiles = lvPictures.ItemsSource as ObservableCollection<StylePictureFile>;
            RadButton btn = sender as RadButton;
            StylePictureFile file = (StylePictureFile)btn.DataContext;
            pfiles.Remove(file);
        }
    }
}
