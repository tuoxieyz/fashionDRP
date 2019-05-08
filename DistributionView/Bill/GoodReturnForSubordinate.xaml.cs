using DistributionModel;
using DistributionViewModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections;
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

namespace DistributionView.Bill
{
    /// <summary>
    /// GoodReturnForSubordinate.xaml 的交互逻辑
    /// </summary>
    public partial class GoodReturnForSubordinate : UserControl
    {
        GoodReturnForSubordinateVM _dataContext = new GoodReturnForSubordinateVM();

        public GoodReturnForSubordinate()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void txtProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var bill = _dataContext.Master;
                if (bill.OrganizationID == default(int))
                {
                    MessageBox.Show("未指定退货机构");
                    return;
                }
                var tb = (TextBox)sender;
                SysProcessView.UIHelper.ProductCodeInput<BillGoodReturn, BillGoodReturnDetails, GoodReturnProductShow>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }           

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((GoodReturnProductShow)btn.DataContext);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var opresult = _dataContext.ValidateWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                return;
            }
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<DistributionProductShow>(gvDatas, _dataContext.Master.BrandID))
                return;
            var result = _dataContext.Save();
            if (result.IsSucceed)
            {
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }

        private void gvDatas_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            gvDatas.CalculateAggregates();
        }
    }
}
