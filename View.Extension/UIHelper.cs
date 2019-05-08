using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataForm;
using System.Windows;
using Telerik.Windows.Controls;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using Model.Extension;
using Telerik.Windows.Controls.GridView;
using ViewModelBasic;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Printing;

namespace View.Extension
{
    public static class UIHelper
    {
        public static void AddOrUpdateRecord<TEntity>(RadDataForm form, ICUDOper<TEntity> dataContext, EditEndingEventArgs e) where TEntity : class
        {
            if (form.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                TEntity entity = (TEntity)form.CurrentItem;
                var result = dataContext.AddOrUpdate(entity);
                MessageBox.Show(result.Message);
                if (!result.IsSucceed)
                {
                    e.Cancel = true;
                }
            }
        }

        public static void DeleteRecord<TEntity>(RadDataForm form, ICUDOper<TEntity> dataContext, CancelEventArgs e) where TEntity : class
        {
            TEntity entity = (TEntity)form.CurrentItem;
            var result = dataContext.Delete(entity);
            MessageBox.Show(result.Message);
            if (!result.IsSucceed)
            {
                e.Cancel = true;
            }
        }

        public static TControl GetDataFormField<TControl>(RadDataForm form, string fieldName) where TControl : class
        {
            //应将RadDataForm中私有的fieldsContentPresenter封装成公共属性来调用（此处标记）
            var fieldsContentPresenter = form.Template.FindName("PART_FieldsContentPresenter", form) as ContentPresenter;
            var lbSize = fieldsContentPresenter.ContentTemplate.FindName(fieldName, fieldsContentPresenter) as TControl;
            return lbSize;
        }

        /// <summary>
        /// 禁用RadDataForm的水平滚动条
        /// <remarks>主要用于使得内部控件能根据容器宽度自绘，在有自动换行的控件时比较有用</remarks>
        /// </summary>
        public static void DisableFormHorizontalScrollBar(RadDataForm form)
        {
            var itemsScrollViewer = form.Template.FindName("PART_ItemsScrollViewer", form) as ScrollViewer;
            if (itemsScrollViewer != null)
                itemsScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        public static T GetAncestor<T>(DependencyObject obj, string name = "") where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T && (string.IsNullOrEmpty(name) || ((T)parent).Name == name))
                {
                    return (T)parent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// [递归]查找子级节点
        /// </summary>
        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        /// <summary>
        /// 根据名称[递归]查找子级节点
        /// </summary>
        public static T GetVisualChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null || child.Name != childName)
                {
                    child = GetVisualChild<T>(v, childName);
                }
                if (child != null && child.Name == childName)//else由于child可能被重新赋值，因此要重新判断，而不能使用else
                {
                    break;
                }
            }
            return child;
        }

        /// <summary>
        /// 递归检查自身及子节点是否有效
        /// </summary>
        public static bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }
            }

            // All dependency objects are valid
            return true;
        }

        public static void ExcelExport(RadGridView gv)
        {
            if (gv.HasItems)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = "xls";
                dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", "xls", "Excel");
                dialog.FilterIndex = 1;

                if (dialog.ShowDialog() == true)
                {
                    using (Stream stream = dialog.OpenFile())
                    {
                        GridViewExportOptions exportOptions = new GridViewExportOptions();
                        exportOptions.Format = ExportFormat.Html;
                        exportOptions.Encoding = Encoding.BigEndianUnicode;
                        exportOptions.ShowColumnFooters = true;
                        exportOptions.ShowColumnHeaders = true;
                        //exportOptions.ShowGroupFooters = true;
                        gv.Export(stream, exportOptions);
                    }
                    MessageBox.Show("导出完毕");
                }
            }
            else
                MessageBox.Show("没有可导出的数据");
        }

        public static void SetGridRowValidBackground(GridViewRow row, bool isValid)
        {
            Border invalidBD = row.Template.FindName("Background_Invalid", row) as Border;
            invalidBD.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
        }

        public static FixedDocument ToFixedDocument(FrameworkElement element, PrintDialog dialog)
        {
            var capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
            var pageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
            var extentSize = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);

            var fixedDocument = new FixedDocument();

            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
            var totalHeight = element.DesiredSize.Height;

            var yOffset = 0d;
            while (yOffset < totalHeight)
            {
                var brush = new VisualBrush(element)
                {
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    ViewboxUnits = BrushMappingMode.Absolute,
                    TileMode = TileMode.None,
                    Viewbox = new Rect(0, yOffset, extentSize.Width, extentSize.Height)
                };

                var pageContent = new PageContent();
                var page = new FixedPage();
                ((IAddChild)pageContent).AddChild(page);

                fixedDocument.Pages.Add(pageContent);
                page.Width = pageSize.Width;
                page.Height = pageSize.Height;

                var canvas = new Canvas();
                FixedPage.SetLeft(canvas, capabilities.PageImageableArea.OriginWidth);
                FixedPage.SetTop(canvas, capabilities.PageImageableArea.OriginHeight);
                canvas.Width = extentSize.Width;
                canvas.Height = extentSize.Height;
                canvas.Background = brush;

                page.Children.Add(canvas);

                yOffset += extentSize.Height;
            }
            return fixedDocument;
        }

        public static void Print(string filePath, object dataContext)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            FixedPage fixedPage = XamlReader.Load(fileStream) as FixedPage;
            fixedPage.DataContext = dataContext;
            Print(fixedPage); 
        }

        public static void Print(FixedPage page)//, string pageName
        {
            //page.UpdateLayout();
            PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();

            //FixedPage若直接打印，则打印出来的内容为空白，因为height为0
            //推测原因：没有render
            //但先保存为文件，或加载到FixedDocument中就能打印，奇怪
            #region
            System.Printing.PrintCapabilities capabilities = defaultPrintQueue.GetPrintCapabilities();
            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
            PageContent content = new PageContent();
            ((IAddChild)content).AddChild(page);
            document.Pages.Add(content);
            #endregion

            #region 根据打印机打印区域缩放page大小
            //System.Printing.PrintCapabilities capabilities = defaultPrintQueue.GetPrintCapabilities();
            ////get scale of the print wrt to screen of WPF visual
            //double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / page.ActualWidth, capabilities.PageImageableArea.ExtentHeight / page.ActualHeight);
            ////Transform the Visual to scale
            //page.LayoutTransform = new ScaleTransform(scale, scale);

            ////get the size of the printer page
            //Size sz = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
            ////update the layout of the visual to the printer page size.
            //page.Measure(sz);
            //page.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
            #endregion

            try
            {
                //string path = SaveXPS(page, pageName);
                //PrintSystemJobInfo xpsPringtJob = defaultPrintQueue.AddJob(pageName + ".xps", path, true);
                XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(defaultPrintQueue);//如此会自动帮我们判定上面AddJob方法的第三个参数
                xpsdw.Write(document);
            }
            catch (Exception e)
            {
                MessageBox.Show("打印失败,失败原因:" + e.Message);
            }
            finally
            {
                defaultPrintQueue.Dispose();
            }
        }

        private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);

        public static void DoEvents()
        {
            DispatcherFrame nestedFrame = new DispatcherFrame();

            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);

            Dispatcher.PushFrame(nestedFrame);

            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;
            frame.Continue = false;
            return null;
        }
    }
}
