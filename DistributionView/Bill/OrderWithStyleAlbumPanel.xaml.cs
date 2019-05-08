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
using System.Data;
using SysProcessViewModel;
using telerik = Telerik.Windows.Controls;
using DistributionViewModel;
using DistributionModel;
using SysProcessModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrderWithStyleAlbum.xaml
    /// </summary>
    public partial class OrderWithStyleAlbumPanel : UserControl
    {
        private List<DataTable> _tables = new List<DataTable>();

        public OrderWithStyleAlbumPanel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OrderWithStyleAlbumPanel_Loaded);
        }

        void OrderWithStyleAlbumPanel_Loaded(object sender, RoutedEventArgs e)
        {
            StylePictureAlbum album = this.DataContext as StylePictureAlbum;
            album.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(style_PropertyChanged);

            this.ConstructQuantityGrid();
        }

        void style_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedStyle")
                this.ConstructQuantityGrid();
        }

        private void ConstructQuantityGrid()
        {
            gvDatas.CommitEdit();
            while (gvDatas.Columns.Count > 1)
                gvDatas.Columns.RemoveAt(gvDatas.Columns.Count - 1);
            StylePictureAlbum album = this.DataContext as StylePictureAlbum;
            var style = album.SelectedStyle;
            if (style != null)
            {
                gvDatas.ItemsSource = null;
                var sizes = style.Sizes.OrderBy(o => o.Code).ToList();
                var table = _tables.FirstOrDefault(o => o.TableName == style.Code);
                if (table == null)
                {
                    table = new DataTable(style.Code);
                    table.Columns.Add(new DataColumn("ColorPhoto", typeof(ProSCPictureBO)));

                    foreach (var sn in sizes)
                    {
                        table.Columns.Add(new DataColumn(sn.Name, typeof(int)));
                    }
                    foreach (var pic in style.Pictures)
                    {
                        DataRow row = table.NewRow();
                        table.Rows.Add(row);
                        row["ColorPhoto"] = pic;
                    }
                    _tables.Add(table);
                }
                foreach (var sn in sizes)
                    gvDatas.Columns.Add(new telerik::GridViewDataColumn() { Header = sn.Name, UniqueName = sn.Name, DataMemberBinding = new Binding(sn.Name), MinWidth = 30 });

                gvDatas.ItemsSource = table.DefaultView;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            StylePictureAlbum album = this.DataContext as StylePictureAlbum;
            DistributionCommonBillVM<BillOrder, BillOrderDetails> orderVM = new DistributionCommonBillVM<BillOrder, BillOrderDetails>();

            var details = orderVM.Details = new List<BillOrderDetails>();
            IEnumerable<int> sids = album.Styles.Select(o => o.ID).ToArray();
            var products = VMGlobal.SysProcessQuery.LinqOP.Search<Product>(o => sids.Contains(o.StyleID)).ToList();
            foreach (var table in _tables)
            {
                var dv = table.DefaultView;
                foreach (DataRowView row in dv)
                {
                    ProSCPictureBO pic = (ProSCPictureBO)row[0];
                    for (int i = 1; i < dv.Table.Columns.Count; i++)
                    {
                        int qua = 0;
                        int.TryParse(row[i].ToString(), out qua);
                        if (qua > 0)
                        {
                            string sname = dv.Table.Columns[i].ColumnName;
                            var sid = VMGlobal.Sizes.Find(o => o.Name == sname).ID;
                            var pid = products.Find(o => o.StyleID == pic.StyleID && o.ColorID == pic.ColorID && o.SizeID == sid).ID;
                            details.Add(new BillOrderDetails { ProductID = pid, Quantity = qua, QuaCancel = 0, QuaDelivered = 0, Status = (int)OrderStatusEnum.NotDelivered });
                        }
                    }
                }
            }
            if (details.Count == 0)
            {
                MessageBox.Show("没有需要保存的数据");
                return;
            }
            orderVM.Master.BrandID = album.BrandID;
            orderVM.Master.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            orderVM.Master.Remark = txtRemark.Text;
            orderVM.Master.Status = (int)OrderStatusEnum.NotDelivered;
            var result = orderVM.Save();
            if (result.IsSucceed)
            {
                MessageBox.Show("保存成功");
                foreach (var table in _tables)
                {
                    var dv = table.DefaultView;
                    foreach (DataRowView row in dv)
                    {
                        for (int i = 1; i < dv.Table.Columns.Count; i++)
                        {
                            int qua = 0;
                            int.TryParse(row[i].ToString(), out qua);
                            if (qua > 0)
                            {
                                row[i] = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }
    }
}
