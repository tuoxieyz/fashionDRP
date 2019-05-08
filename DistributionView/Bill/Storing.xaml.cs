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
using DistributionModel;
using DistributionViewModel;

using DomainLogicEncap;
using Telerik.Windows.Controls;
using SysProcessViewModel;
using ERPViewModelBasic;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for Storing.xaml
    /// </summary>
    public partial class Storing : UserControl
    {
        BillStoringVM _dataContext = new BillStoringVM();

        public Storing()
        {            
            this.DataContext = _dataContext;
            InitializeComponent();
            var storages = StorageInfoVM.Storages;
            cbxStorage.ItemsSource = storages;
            if (storages.Count == 1)
                _dataContext.Master.StorageID = storages[0].ID;
            cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            if (VMGlobal.PoweredBrands.Count == 1)
                _dataContext.Master.BrandID = VMGlobal.PoweredBrands[0].ID;
#if UniqueCode
            gvDatas.SetResourceReference(GridViewDataControl.RowDetailsTemplateProperty, "uniqueCodeDetailsTemplate");
#endif
        }

        private void txtProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tb = (TextBox)sender;
                SysProcessView.UIHelper.ProductCodeInput<BillStoring, BillStoringDetails, DistributionProductShow>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void InitDataContext()
        {
            _dataContext.Init();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var bill = _dataContext.Master;
            if (bill.StorageID == 0)
            {
                MessageBox.Show("请选择入库仓库");
                return;
            }
            if (bill.BrandID == default(int))
            {
                MessageBox.Show("未指定入库品牌");
                return;
            }
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<DistributionProductShow>(gvDatas, bill.BrandID))
                return;                      
            var result = _dataContext.Save();
            if (result.IsSucceed)
            {
                MessageBox.Show("保存成功");
                InitDataContext();
            }
            else
            {
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((DistributionProductShow)btn.DataContext);
        }
    }
}
