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
using Telerik.Windows.Controls;
using DistributionViewModel;

namespace DistributionView.Reports.Charts
{
    /// <summary>
    /// Interaction logic for RevenueActualvsTarget.xaml
    /// </summary>
    public partial class RevenueActualvsTarget : UserControl
    {
        public RevenueActualvsTarget()
        {
            InitializeComponent();
        }

        private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RadDatePicker datePicker = (RadDatePicker)sender;
                DateTime date = (DateTime)e.AddedItems[0];
                datePicker.DateTimeText = date.ToString("yyyy-MM");
            }
        }

        private void ChartSelectionBehavior_SelectionChanged(object sender, Telerik.Windows.Controls.ChartView.ChartSelectionChangedEventArgs e)
        {
            if (e.AddedPoints.Count > 0)
            {
                var addedPoint = e.AddedPoints[0];
                RetailMonthTagetBO bo = (RetailMonthTagetBO)addedPoint.DataItem;
                this.SetSaleDetails(bo);
            }
        }

        private void RadGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var addedPoint = e.AddedItems[0];
                RetailMonthTagetBO bo = (RetailMonthTagetBO)addedPoint;
                this.SetSaleDetails(bo);
            }
        }

        private void SetSaleDetails(RetailMonthTagetBO bo)
        {
            var minDate = bo.SaleDetails.Min(o => o.CreateDate);
            //if (minDate.Day != 1)
            //{
            //BillRetailBO retail = new BillRetailBO { CreateDate = new DateTime(minDate.Year, minDate.Month, 1), CostMoney = 0 };
            //bo.SaleDetails.Insert(0, retail);//从月份第1天开始  
            xAxis.Minimum = new DateTime(minDate.Year, minDate.Month, 1);
            //}
            //var r = bo.SaleDetails.Find(o => o.CreateDate.Day == 2);
            //if (r == null)
            //{
            //    //BillRetailBO retail = new BillRetailBO { CreateDate = new DateTime(minDate.Year, minDate.Month, 2), CostMoney = 0 };
            //    //bo.SaleDetails.Insert(1, retail);//为了界面上的DateTimeContinuousAxis控件的时间轴显示间隔为1天   
            //}
            //var maxDate = bo.SaleDetails.Max(o=>o.CreateDate);
            var days = DateTime.DaysInMonth(minDate.Year, minDate.Month);
            //if (maxDate.Day != days)
            //{
            //BillRetailBO retail = new BillRetailBO { CreateDate = new DateTime(minDate.Year, minDate.Month, days), CostMoney = 0 };
            //bo.SaleDetails.Add(retail);//自月份最后一天结束 
            xAxis.Maximum = new DateTime(minDate.Year, minDate.Month, days);
            //}            
            barDetails.ItemsSource = bo.SaleDetails;
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }
    }
}
