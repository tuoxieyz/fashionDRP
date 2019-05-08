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
using Manufacturing.ViewModel;
using ManufacturingModel;
using Telerik.Windows.Controls.GridView;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for ProductPlan.xaml
    /// </summary>
    public partial class ProductPlan : UserControl
    {
        BillProductPlanVM _dataContext = new BillProductPlanVM();

        public ProductPlan()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void txtProductCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var bill = _dataContext.Master;
                var tb = (TextBox)sender;
                SysProcessView.UIHelper.ProductCodeInput<BillProductPlan, BillProductPlanDetails, ProductForProduceBrush>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductForProduceBrush)btn.DataContext);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var opresult = _dataContext.ValidateWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                return;
            }
            var bill = _dataContext.Master;
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<ProductForProduceBrush>(gvDatas, bill.BrandID))
                return;
            var orderlimited = _dataContext.CheckOrderlimited();
            if (orderlimited.Count() > 0)
            {
                var pids = orderlimited.Select(o => o.ProductID);
                SysProcessView.UIHelper.TraverseGridViewData<ProductForProduceBrush>(gvDatas, p =>
                {
                    if (pids.Contains(p.ProductID))
                    {
                        var row = gvDatas.ItemContainerGenerator.ContainerFromItem(p) as GridViewRow;
                        View.Extension.UIHelper.SetGridRowValidBackground(row, false);
                    }
                });
                var dr = MessageBox.Show("列表中有SKU的计划生产量超出了剩余订单量（即未列入生产计划的订单量）,是否继续？", "警告", MessageBoxButton.YesNo);
                if (dr == MessageBoxResult.No)
                    return;
            }
            var result = _dataContext.Save();
            if (result.IsSucceed)
            {
                MessageBox.Show("保存成功");
                _dataContext.Init();
            }
            else
            {
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }
    }
}
