using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using Telerik.Windows.Controls;
using DomainLogicEncap;
using Telerik.Windows.Controls.GridView;
using DBAccess;
using Telerik.Windows.Controls.Data.DataForm;
using Model.Extension;
using SysProcessViewModel;
using ERPViewModelBasic;
using ViewModelBasic;
using System.Windows.Data;
using Telerik.Windows.Data;
using System.Data;

namespace SysProcessView
{
    public static class UIHelper
    {
        /// <summary>
        /// 根据传入的字符串标示获取地区、省份或机构类型
        /// </summary>
        /// <param name="apt">AreaID、ProvienceID或OrganizationTypeID</param>
        public static void SetAPTForFilter(string apt, RadComboBox cbx)
        {
            switch (apt)
            {
                case "AreaID":
                    cbx.ItemsSource = OrganizationLogic.GetAreas();
                    break;
                case "ProvienceID":
                    cbx.ItemsSource = OrganizationLogic.GetProvinces();
                    break;
                case "OrganizationTypeID":
                    cbx.ItemsSource = OrganizationLogic.GetOrganizationTypes(VMGlobal.CurrentUser.OrganizationID);
                    break;
            }
        }

        public static void BillExportExcel(string title, RadGridView gv, object item)
        {
            var row = (GridViewRow)gv.ItemContainerGenerator.ContainerFromItem(item);//View.Extension.UIHelper.GetAncestor<GridViewRow>(sender as RadButton);
            row.DetailsVisibility = Visibility.Visible;
            var detailsPresenter = row.ChildrenOfType<DetailsPresenter>().FirstOrDefault();
            // same as e.DetailsElement from gridView_RowDetailsVisibilityChanged 
            var gvDetails = (RadGridView)detailsPresenter.Content;

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = "xls";
            dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", "xls", "Excel");
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (System.IO.Stream stream = dialog.OpenFile())
                    {
                        System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
                        writer.WriteLine(@"<table border=""1"">");
                        writer.WriteLine(@"<tr><td style=""background-color:#CCC;text-align:center;""><b>");
                        writer.WriteLine(title);
                        writer.WriteLine("</b></td></tr><tr><td>");

                        foreach (var col in gv.Columns)
                        {
                            if (col.Header != null)//过滤没有列头（如标识列）的列
                            {
                                var colname = col.Header.ToString();
                                if (colname != "操作" && col is IExportableColumn && col.IsVisible)
                                {
                                    writer.Write(@"<b>{0}:</b> {1} <br />", colname, ((IExportableColumn)col).GetCellContent(item));
                                }
                            }
                        }
                        writer.WriteLine("</td></tr><tr><td>");
                        writer.Flush();
                        GridViewExportOptions exportOptions = new GridViewExportOptions();
                        exportOptions.Format = ExportFormat.Html;
                        exportOptions.ShowColumnHeaders = true;
                        gvDetails.Export(stream, exportOptions);
                        writer.WriteLine("</td></tr></table>");
                        writer.Flush();
                    }
                    MessageBox.Show("导出完毕");
                }
                catch (Exception e)
                {
                    MessageBox.Show("导出失败,失败原因:\n" + e.Message);
                }
            }
        }

        public static void PrintBill(string title, RadGridView gv, object item)
        {
            var dialog = new PrintDialog();
            var viewer = new DocumentViewer();
            viewer.Document = View.Extension.UIHelper.ToFixedDocument(new BillPrintTemplate(title, gv, item), dialog);
            dialog.PrintDocument(viewer.Document.DocumentPaginator, title);
        }

        //public static void ProductCodeInput<T, TDetail>(System.Windows.Controls.TextBox txtProductCode, DistributionCommonBillVM<T, TDetail> context, UserControl control)
        //    where T : BillBase, new()
        //    where TDetail : BillDetailBase
        //{
        //    ProductCodeInput<T, TDetail, ProductForBrush>(txtProductCode, context, control);
        //}

        /// <summary>
        /// 扫单
        /// </summary>
        public static void ProductCodeInput<T, TDetail, TForBrush>(TextBox txtProductCode, BillVMBase<T, TDetail, TForBrush> context, UserControl control)
            where T : BillBase, new()
            where TDetail : BillDetailBase
            where TForBrush : ProductShow, new()
        {
            string code = txtProductCode.Text.Trim();
            if (!string.IsNullOrEmpty(code))
            {
                try
                {
                    context.ProductCodeInput(code, datas =>
                    {
                        ProStyleQuantitySetWin win = new ProStyleQuantitySetWin();
                        win.DataContext = datas;
                        win.Owner = View.Extension.UIHelper.GetAncestor<Window>(control);
                        win.SetCompletedEvent += delegate
                        {
                            context.AddRangeToItems(datas);                            
                        };
                        win.ShowDialog();
                    });
                    System.Media.SystemSounds.Asterisk.Play();
                }
                catch (Exception ep)
                {
                    System.Media.SystemSounds.Hand.Play();
                    MessageBox.Show(ep.Message);
                }
            }
            txtProductCode.Clear();
        }

        /// <summary>
        /// 遍历表格内数据
        /// </summary>
        public static void TraverseGridViewData<T>(RadGridView gvDatas, Action<T> action) where T : ProductShow
        {
            foreach (var item in gvDatas.Items)
            {
                var product = (T)item;
                if (product.Quantity != 0)
                {
                    action(product);
                }
            }
        }

        /// <summary>
        /// 列表数据转换单据明细实体数据前验证品牌
        /// </summary>
        public static bool CheckGridViewDataWithBrand<T>(RadGridView gvDatas, int brandID) where T : ProductShow
        {
            ResetGridRowBackground(gvDatas);
            List<int> pids = new List<int>();//取得不属于选定品牌的成品ID集合
            TraverseGridViewData<T>(gvDatas, p =>
            {
                if (p.BrandID != brandID)
                {
                    pids.Add(p.ProductID);
                    var row = gvDatas.ItemContainerGenerator.ContainerFromItem(p) as GridViewRow;
                    //row.Background = (Brush)Application.Current.FindResource("GridViewRowThrowColor");
                    View.Extension.UIHelper.SetGridRowValidBackground(row, false);
                }
            });
            if (pids.Count > 0)
            {
                var yn = MessageBox.Show("单据中有不属于选定品牌的成品条码,这些条码将不被保存,是否继续?", "提示", MessageBoxButton.YesNo);
                if (yn == MessageBoxResult.No)
                    return false;
                for (int i = 0; i < gvDatas.Items.Count; i++)
                {
                    var item = (T)gvDatas.Items[i];
                    if (pids.Contains(item.ProductID))
                    {
                        //对于绑定到数据集的gvDatas，这样也行,会不会对数据源产生影响呢？我记得有个Remove操作会抛出异常(其实是telerik控件做了相关处理)
                        // gvDatas.Items.Remove(item);会抛异常
                        //这么写会影响数据源,不错
                        gvDatas.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
            return true;
        }

        ///// <summary>
        ///// 列表数据转换单据明细实体数据后验证品牌
        ///// </summary>
        //public static bool CheckDetailsWithBrand<TDetails>(List<TDetails> details, int brandID, RadGridView gvDatas) where TDetails : BillDetailBase
        //{
        //    if (details.Count == 0)
        //    {
        //        MessageBox.Show("没有需要保存的数据");
        //        return false;
        //    }
        //    var pids = details.Select(o => o.ProductID);
        //    //取得不属于选定品牌的成品ID集合
        //    pids = (new QueryGlobal("DistributionConstr")).LinqOP.Search<ViewProduct, int>(o => o.ProductID, o => pids.Contains(o.ProductID) && o.BrandID != brandID).ToList();
        //    ResetGridRowBackground(gvDatas);
        //    if (pids.Count() > 0)
        //    {
        //        UIHelper.TraverseGridViewData<ProductForBrush>(gvDatas, p =>
        //        {
        //            if (pids.Contains(p.ProductID))
        //            {
        //                var row = gvDatas.ItemContainerGenerator.ContainerFromItem(p) as GridViewRow;
        //                row.Background = (Brush)Application.Current.FindResource("GridViewRowThrowColor");
        //            }
        //        });
        //        var yn = MessageBox.Show("单据中有不属于选定品牌的成品条码,这些条码将不被保存,是否继续?", "提示", MessageBoxButton.YesNo);
        //        if (yn == MessageBoxResult.No)
        //            return false;
        //        details.RemoveAll(o => pids.Contains(o.ProductID));
        //        if (details.Count == 0)
        //        {
        //            MessageBox.Show("没有需要保存的数据");
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        private static void ResetGridRowBackground(RadGridView gvDatas)
        {
            //var brush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            UIHelper.TraverseGridViewData<ProductShow>(gvDatas, p =>
            {
                var row = gvDatas.ItemContainerGenerator.ContainerFromItem(p) as GridViewRow;
                //row.Background = brush;
                View.Extension.UIHelper.SetGridRowValidBackground(row, true);
            });
        }

        public static void AddOrUpdateRecord<TEntity>(RadDataForm form, ICUDOper<TEntity> dataContext, EditEndingEventArgs e) where TEntity : class
        {
            if (form.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                TEntity entity = (TEntity)form.CurrentItem;
                if (entity is IDEntity && ((IDEntity)entity).ID == default(int))
                {
                    CreatedData cd = entity as CreatedData;
                    if (cd != null)
                    {
                        cd.CreatorID = VMGlobal.CurrentUser.ID;
                        cd.CreateTime = DateTime.Now;
                    }
                }
                var result = dataContext.AddOrUpdate(entity);
                MessageBox.Show(result.Message);
                if (!result.IsSucceed)
                {
                    e.Cancel = true;
                }
            }
        }

        public static void ToggleShowEqualFilterOperatorOnly(FrameworkElement element, bool extraCondition = false)
        {
            element.Loaded += delegate
            {
                var bd = View.Extension.UIHelper.GetAncestor<Border>(element, "InnerBorder");
                if (bd != null)
                {
                    var cbx = View.Extension.UIHelper.GetVisualChild<RadComboBox>(bd, "PART_SimpleFilterOperatorComboBox");
                    if (cbx != null)
                    {
                        var view = CollectionViewSource.GetDefaultView(cbx.ItemsSource);
                        if (element is RadComboBox || element is OrganizationSelector || extraCondition)
                            view.Filter = new Predicate<object>(o =>
                            {
                                return ((FilterOperator)o) == FilterOperator.IsEqualTo;
                            });
                        else
                            view.Filter = null;
                    }
                }
            };
        }

        public static void TransferSizeToHorizontal(RadGridView gv)
        {
            System.ComponentModel.TypeDescriptor.GetProperties(gv)["ItemsSource"].AddValueChanged(gv, new EventHandler(gv_ItemsSourceChanged));
            var skuCol = gv.Columns.FirstOrDefault<Telerik.Windows.Controls.GridViewColumn>(o => o.Header.ToString() == "SKU码");
            if (skuCol != null)
                gv.Columns.Remove(skuCol);
            var sizeCol = gv.Columns.FirstOrDefault<Telerik.Windows.Controls.GridViewColumn>(o => o.Header.ToString() == "尺码");
            if (sizeCol != null)
            {
                int index = gv.Columns.IndexOf(sizeCol);
                gv.Columns.RemoveAt(index);
                foreach (var size in VMGlobal.Sizes)
                {
                    var col = new GridViewDataColumn() { Header = size.Name, DataMemberBinding = new Binding(size.Name), Tag = "SizeCol" };//, Name = size.Name
                    gv.Columns.Insert(index, col);
                    index++;
                }
            }
        }

        static void gv_ItemsSourceChanged(object sender, EventArgs e)
        {
            RadGridView gv = sender as RadGridView;
            var dt = gv.ItemsSource as DataTable;
            if (dt != null)
            {
                var sizeCols = gv.Columns.Where<Telerik.Windows.Controls.GridViewColumn>(o => o.Tag == "SizeCol").ToArray();
                foreach (var col in sizeCols)
                {
                    if (dt.Columns.Contains(col.Header.ToString()))
                        col.IsVisible = true;
                    else
                        col.IsVisible = false;
                }
            }
        }

        public static void BindGridViewWithSizeToHorizontal(RadGridView gv, IEnumerable<ProductShow> products)
        {
            TransferSizeToHorizontal(gv);
            gv.ItemsSource = new BillReportHelper().TransferSizeToHorizontal<ProductShow>(products);
        }

        #region 附加属性——尺码横排

        //public static readonly DependencyProperty IsSizeHorizontalProperty =
        //      DependencyProperty.RegisterAttached("IsSizeHorizontal",
        //      typeof(bool),
        //      typeof(UIHelper),
        //      new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSizeHorizontalChanged)));

        //public static bool GetIsSizeHorizontal(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(IsSizeHorizontalProperty);
        //}

        //public static void SetIsSizeHorizontal(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(IsSizeHorizontalProperty, value);
        //}

        //private static void OnIsSizeHorizontalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    var gv = o as RadGridView;
        //    if (gv != null)
        //    {
        //        bool isSizeHorizontal = (bool)e.NewValue;
        //        if (isSizeHorizontal)
        //            TransferSizeToHorizontal(gv);
        //    }
        //}

        #endregion
    }
}
