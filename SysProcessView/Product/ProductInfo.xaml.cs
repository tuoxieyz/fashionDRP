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
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls.Calendar;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Data.DataForm;
using SysProcessModel;
using System.Net;
using System.Configuration;
using IWCFService;
using System.ServiceModel;
using System.IO;
using DomainLogicEncap;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for ProductInfo.xaml
    /// </summary>
    public partial class ProductSetting : UserControl
    {
        public ProductSetting()
        {

            InitializeComponent();
            myRadDataForm.CommandButtonsVisibility = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.成品资料) & (~DataFormCommandButtonsVisibility.Delete);
            myRadDataForm.Loaded += delegate
            {
                View.Extension.UIHelper.DisableFormHorizontalScrollBar(myRadDataForm);
            };
        }

        private void radDataFilter_EditorCreated(object sender, EditorCreatedEventArgs e)
        {
            //ProductListVM context = this.DataContext as ProductListVM;

            switch (e.ItemPropertyDefinition.PropertyName)
            {
                case "BrandID":
                    // This is a custom editor specified through the EditorTemplateSelector.
                    RadComboBox cbxBrand = (RadComboBox)e.Editor;
                    cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
                    break;
                case "Year":
                    // This is a default editor.
                    //RadDateTimePicker dateTimePickerEditor = (RadDateTimePicker)e.Editor;
                    ////dateTimePickerEditor.InputMode = Telerik.Windows.Controls.InputMode.DatePicker;
                    //dateTimePickerEditor.DateSelectionMode = DateSelectionMode.Year;
                    //dateTimePickerEditor.IsTooltipEnabled = true;
                    //dateTimePickerEditor.ErrorTooltipContent = "输入格式不正确";
                    //dateTimePickerEditor.DateTimeWatermarkContent = "选择年份";
                    //dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                    //{
                    //    DateTime date = (DateTime)ee.AddedItems[0];
                    //    dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    //};
                    //break;
                    RadDatePicker dateTimePickerEditor = (RadDatePicker)e.Editor;
                    //dateTimePickerEditor.InputMode = Telerik.Windows.Controls.InputMode.DatePicker;
                    dateTimePickerEditor.SelectionChanged += (ss, ee) =>
                    {
                        DateTime date = (DateTime)ee.AddedItems[0];
                        dateTimePickerEditor.DateTimeText = date.Year.ToString();
                    };
                    break;
                case "BoduanID":
                    RadComboBox cbxBoduan = (RadComboBox)e.Editor;
                    cbxBoduan.ItemsSource = VMGlobal.Boduans;
                    break;
                case "Quarter":
                    RadComboBox cbxQuarter = (RadComboBox)e.Editor;
                    cbxQuarter.ItemsSource = VMGlobal.Quarters;
                    break;
                case "SizeID":
                    RadComboBox cbxSize = (RadComboBox)e.Editor;
                    cbxSize.ItemsSource = VMGlobal.Sizes;
                    break;
                case "NameID":
                    RadComboBox cbxName = (RadComboBox)e.Editor;
                    cbxName.ItemsSource = VMGlobal.ProNames;
                    break;
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);

        }

        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    if (myRadDataForm != null && myRadDataForm.CanCancelEdit)
        //        myRadDataForm.CancelEdit();
        //    int totalCount = 0;
        //    ProductListVM context = this.DataContext as ProductListVM;            
        //    context.GetProducts(radDataFilter.FilterDescriptors, pager.PageIndex, pager.PageSize, ref totalCount);
        //    pager.ItemCount = totalCount;
        //}

        private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DateTime date = (DateTime)e.AddedItems[0];
                RadDatePicker picker = sender as RadDatePicker;
                picker.DateTimeText = date.Year.ToString();
            }
        }

        private ItemsControl GetSizeListBox()
        {
            var lbSize = View.Extension.UIHelper.GetDataFormField<ItemsControl>(myRadDataForm, "lbSize");
            return lbSize;
        }

        private ItemsControl GetColorListBox()
        {
            //应将RadDataForm中私有的fieldsContentPresenter封装成公共属性来调用（涉及到改第三方源码，此处标记）
            //var fieldsContentPresenter = myRadDataForm.Template.FindName("PART_FieldsContentPresenter", myRadDataForm) as ContentPresenter;
            //var lbColor = myRadDataForm.EditTemplate.FindName("lbColor", fieldsContentPresenter) as ListBox;
            //fieldsContentPresenter.ContentTemplate会根据状态不同自动切换相应的Template
            // var lbColor = fieldsContentPresenter.ContentTemplate.FindName("lbColor", fieldsContentPresenter) as ListBox;
            var lbColor = View.Extension.UIHelper.GetDataFormField<ItemsControl>(myRadDataForm, "lbColor");
            return lbColor;
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                ProductListVM context = this.DataContext as ProductListVM;
                //if (!UploadPictureForCurrentStyle())
                //{
                //    var isContinue = MessageBox.Show("款式图片未成功上传，是否继续保存成品信息？", "提示", MessageBoxButton.YesNo);
                //    if (isContinue == MessageBoxResult.No)
                //        return;
                //}

                var lbColor = GetColorListBox();
                var colorSets = lbColor.ItemsSource as List<ProColorForSet>;
                var lbSize = GetSizeListBox();
                var sizeSets = lbSize.ItemsSource as List<ProSizeForSet>;
                var style = (ProStyleBO)myRadDataForm.CurrentItem;
                style.Sizes = sizeSets.FindAll(cs => cs.IsHold).ToList();
                style.Colors = colorSets.FindAll(cs => cs.IsHold).ToList();

                UIHelper.AddOrUpdateRecord(myRadDataForm, context, e);
            }
            //else if (e.EditAction == EditAction.Cancel)
            //{
            //    var item = (ProStyleBO)myRadDataForm.CurrentItem;
            //    if (item.ID != default(int))//编辑状态
            //    {
            //        myRadDataForm.CurrentItem = context.GetProduct(item.ID);
            //        e.Cancel = true;
            //    }
            //}
        }

        private void SizeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var lbSize = this.GetSizeListBox();
            var sizes = lbSize.ItemsSource as List<ProSizeForSet>;
            sizes.ForEach(s => s.IsHold = true);
        }

        private void SizeCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            var lbSize = this.GetSizeListBox();
            var sizes = lbSize.ItemsSource as List<ProSizeForSet>;
            if (myRadDataForm.Mode == RadDataFormMode.AddNew)
            {
                sizes.ForEach(s => s.IsHold = false);
            }
            else if (myRadDataForm.Mode == RadDataFormMode.Edit)
            {
                ProStyleBO style = myRadDataForm.CurrentItem as ProStyleBO;
                var sizesHold = style.Sizes;

                sizes.ForEach(s =>
                {
                    if (!sizesHold.Any(sh => sh.ID == s.ID))
                    {
                        s.IsHold = false;
                    }
                });
                //BindingExpression binding = lbSize.GetBindingExpression(ListBox.ItemsSourceProperty);
                //binding.UpdateTarget();
            }
        }

        private void ColorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var lbColor = this.GetColorListBox();
            var colors = lbColor.ItemsSource as List<ProColorForSet>;
            colors.ForEach(s => s.IsHold = true);
        }

        private void ColorCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            var lbColor = this.GetColorListBox();
            var colors = lbColor.ItemsSource as List<ProColorForSet>;
            if (myRadDataForm.Mode == RadDataFormMode.AddNew)
            {
                colors.ForEach(c => c.IsHold = false);
            }
            else if (myRadDataForm.Mode == RadDataFormMode.Edit)
            {
                ProStyleBO style = myRadDataForm.CurrentItem as ProStyleBO;
                var colorsHold = style.Colors;

                colors.ForEach(c =>
                {
                    if (!colorsHold.Any(ch => ch.ID == c.ID))
                    {
                        c.IsHold = false;
                    }
                });
                //BindingExpression binding = lbSize.GetBindingExpression(ListBox.ItemsSourceProperty);
                //binding.UpdateTarget();
            }

        }

        //private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    MessageBox.Show("成品资料信息不可删除");
        //    e.Cancel = true;
        //}

        //private void RadDataPager_PageIndexChanged(object sender, PageIndexChangedEventArgs e)
        //{
        //    this.btnSearch_Click(null, null);
        //}

#region 注释但保留为了以后查阅的代码

        //private void btnPicSelect_Click(object sender, RoutedEventArgs e)
        //{
        //    //创建＂打开文件＂对话框 
        //    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        //    //设置文件类型过滤
        //    dlg.Filter = "图片|*.jpg;*.png;*.gif;*.bmp;*.jpeg";
        //    // 调用ShowDialog方法显示＂打开文件＂对话框
        //    Nullable<bool> result = dlg.ShowDialog();
        //    if (result == true)
        //    {
        //        //获取所选文件名完整路径
        //        string filename = dlg.FileName;
        //        var item = (ProStyle)myRadDataForm.CurrentItem;
        //        item.PictureUrl = filename;
        //        //var btn = sender as Button;
        //        //var tbPath = btn.Parent.FindChildByType<TextBox>();
        //        //tbPath.Text = filename;
        //        ////在image中预览所选图片 
        //        //BitmapImage image = new BitmapImage(new Uri(filename));
        //        //var imgStyle = GetImageStyle();
        //        //imgStyle.Source = image;
        //    }
        //}

        /// <summary>
        /// 上传款式图片
        /// </summary>
        /// <returns>是否上传成功或是否不用上传</returns>
        //private bool UploadPictureForCurrentStyle()
        //{
        //    var style = (ProStyle)myRadDataForm.CurrentItem;
        //    if (string.IsNullOrEmpty(style.PictureUrl) || style.PictureUrl.StartsWith("http"))
        //        return true;
        //    FileInfo localFile = new FileInfo(style.PictureUrl);
        //    //文件夹+文件名称
        //    var fileName = style.Year + style.Quarter.ToString("00") + "\\" + style.Code + localFile.Extension;
        //    FileToUpload file = new FileToUpload
        //    {
        //        FileName = "StylePicture\\" + fileName,
        //        FileContent = localFile.OpenRead()
        //    };
        //    BoolMessage result = null;
        //    using (ChannelFactory<IUploadService> channelFactory = new ChannelFactory<IUploadService>("UploadSVC"))
        //    {
        //        IUploadService service = channelFactory.CreateChannel();
        //        result = service.SaveUploadedFile(file);
        //    }
        //    file.FileContent.Close();
        //    if (result.Flag)
        //    {
        //        var uri = ConfigurationManager.AppSettings["StylePictureUploadUri"] + fileName.Replace("\\", "/");
        //        style.PictureUrl = uri + "?randnum=" + new Random().NextDouble().ToString();//关于为什么要在uri后面多加一个随机数的问题，请参看项目手记第98条
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        //private Image GetImageStyle()
        //{
        //    var fieldsContentPresenter = myRadDataForm.Template.FindName("PART_FieldsContentPresenter", myRadDataForm) as ContentPresenter;
        //    var imgStyle = fieldsContentPresenter.ContentTemplate.FindName("imgStyle", fieldsContentPresenter) as Image;
        //    return imgStyle;
        //}

        //private void btnPicUpload_Click(object sender, RoutedEventArgs e)
        //{
        //    var style = (ProStyle)myRadDataForm.CurrentItem;
        //    if (style.ID == default(int))
        //    {
        //        MessageBox.Show("上传图片前请先保存成品信息");
        //        return;
        //    }
        //    //var imgStyle = GetImageStyle();
        //    //if (imgStyle.Source == null)
        //    var btn = sender as Button;
        //    var tbPath = btn.Parent.FindChildByType<TextBox>();
        //    if (string.IsNullOrEmpty(tbPath.Text))
        //    {
        //        MessageBox.Show("请先选择要上传的图片");
        //        return;
        //    }
        //    FileInfo localFile = new FileInfo(tbPath.Text);
        //    //WebClient webClient = new WebClient(); 
        //    //var uploadUri = ConfigurationManager.AppSettings["StylePictureUploadUri"];
        //    //webClient.UploadFile(uploadUri + "IMG_0681.JPG", "PUT", @"J:\photo\IMG_0681.JPG");
        //    //HttpWebRequest
        //    var fileName = style.Year + style.Quarter.ToString("00") + "\\" + style.Code + localFile.Extension;
        //    FileToUpload file = new FileToUpload
        //    {
        //        FileName = "StylePicture\\" + fileName,
        //        FileContent = localFile.OpenRead()
        //    };
        //    BoolMessage result = null;
        //    using (ChannelFactory<IUploadService> channelFactory = new ChannelFactory<IUploadService>("UploadSVC"))
        //    {
        //        IUploadService service = channelFactory.CreateChannel();
        //        result = service.SaveUploadedFile(file);
        //    }
        //    file.FileContent.Close();
        //    MessageBox.Show(result.Flag ? "上传成功" : "上传失败，请联系网络管理员");
        //    if (result.Flag)
        //    {
        //        var uri = ConfigurationManager.AppSettings["StylePictureUploadUri"] + fileName;
        //        style.PictureUrl = uri;
        //        ProductListVM context = this.DataContext as ProductListVM;
        //        context.Query.LinqOP.Update<ProStyle>(style);

        //        //myRadDataForm.CommitEdit();//回到只读状态
        //    }
        //}

#endregion
    }
}
