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
using System.Windows.Shapes;
using telerik = Telerik.Windows.Controls;
using ERPViewModelBasic;
using System.Data;
using SysProcessViewModel;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for ProStyleQuantitySetWin.xaml
    /// </summary>
    public partial class ProStyleQuantitySetWin : Window
    {
        public event Action SetCompletedEvent;

        private IEnumerable<ProductShow> Context
        {
            get
            {
                return (IEnumerable<ProductShow>)this.DataContext;
            }
        }

        public ProStyleQuantitySetWin()
        {
            InitializeComponent();

            gvDatas.CellEditEnded += new EventHandler<telerik.GridViewCellEditEndedEventArgs>(gvDatas_CellEditEnded);
            this.Loaded += delegate
            {
                //var sizes = Context.OrderBy(o => o.SizeCode).Select(o => o.SizeName).Distinct().ToList();
                //foreach (var sn in sizes)
                //{
                //    gvDatas.Columns.Add(new telerik::GridViewDataColumn() { Header = sn, Name = sn, Width = 50, DataMemberBinding = new Binding(sn) });
                //}
                //var colors = Context.OrderBy(o => o.ColorCode).Select(o => o.ColorCode).Distinct().ToList();
                //foreach (var cc in colors)
                //{
                //    //Add(new {})貌似gridview绑定到匿名类型，将不会为cell自动产生编辑器
                //    //而Add(ExpandoObject对象)将不能更新源数据
                //    //对未绑定属性或绑定到不存在属性的列，也不会为其中的cell保存更新的值
                //    gvDatas.Items.Add(new QuantitySetForProStyle { ColorCode = cc });
                //}
                //gvDatas.BeginEdit();//默认为第一个能编辑的cell
                this.ConstructQuantityGrid();
            };
        }        

        private void ConstructQuantityGrid()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("ColorCode", typeof(string)));
            table.Columns.Add(new DataColumn("ColorName", typeof(string)));

            var sizes = Context.OrderBy(o => o.SizeCode).Select(o => o.SizeName).Distinct().ToList();
            foreach (var sn in sizes)
            {
                table.Columns.Add(new DataColumn(sn, typeof(int)));
                gvDatas.Columns.Add(new telerik::GridViewDataColumn() { Header = sn, UniqueName = sn, Width = 50, DataMemberBinding = new Binding(sn) });
            }
            var colors = Context.OrderBy(o => o.ColorCode).Select(o => o.ColorCode).Distinct().ToList();
            foreach (var cc in colors)
            {
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                row["ColorCode"] = cc;
                row["ColorName"] = VMGlobal.Colors.Find(o => o.Code == cc).Name;
            }
            gvDatas.ItemsSource = table.DefaultView;//一定要用DataView，否则不能编辑,shit
            gvDatas.BeginEdit();//默认为第一个能编辑的cell
        }

        void gvDatas_CellEditEnded(object sender, telerik.GridViewCellEditEndedEventArgs e)
        {
            int value = 0;
            int.TryParse(e.NewData.ToString(), out value);
            //var item = (QuantitySetForProStyle)e.Cell.ParentRow.Item;
            var row = (DataRowView)e.Cell.ParentRow.Item;
            var product = Context.FirstOrDefault(o => o.ColorCode == row[0].ToString() && o.SizeName == e.Cell.Column.UniqueName);
            if(product != null)
                product.Quantity = value;
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            if (SetCompletedEvent != null)
            {
                SetCompletedEvent();
            }
            this.Close();
        }
    }
}
