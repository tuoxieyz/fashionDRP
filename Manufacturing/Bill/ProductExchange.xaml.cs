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
using ERPViewModelBasic;
using Telerik.Windows.Controls.GridView;

namespace Manufacturing
{
    /// <summary>
    /// Interaction logic for ProductExchange.xaml
    /// </summary>
    public partial class ProductExchange : UserControl
    {
        BillProductExchangeVM _dataContext = new BillProductExchangeVM();

        public ProductExchange()
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
                SysProcessView.UIHelper.ProductCodeInput<BillProductExchange, BillProductExchangeDetails, ProductShow>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductShow)btn.DataContext);
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
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<ProductShow>(gvDatas, bill.BrandID))
                return;

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
