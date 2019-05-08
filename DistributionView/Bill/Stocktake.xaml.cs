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
using DistributionViewModel;
using DistributionModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using DomainLogicEncap;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for Stocktake.xaml
    /// </summary>
    public partial class Stocktake : UserControl
    {
        BillStocktakeVM _dataContext = new BillStocktakeVM();

        public Stocktake()
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
            this.btnSave.Click += (sender, e) =>
            {
                btnSave.IsEnabled = false;
                this.Save();
                btnSave.IsEnabled = true;
            };
#if UniqueCode
            gvDatas.SetResourceReference(GridViewDataControl.RowDetailsTemplateProperty, "uniqueCodeDetailsTemplate");
#endif
            //bool isFirstLoad = true;
            //this.Loaded += delegate
            //{
            //    if (isFirstLoad)
            //    {
            //        isFirstLoad = false;
            //        txtProductCode.KeyDown += new KeyEventHandler(txtProductCode_KeyUp);
            //    }
            //};
        }

        private void txtProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SysProcessView.UIHelper.ProductCodeInput<BillStocktake, BillStocktakeDetails, DistributionProductShow>(txtProductCode, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((DistributionProductShow)btn.DataContext);
        }

        private void Save()
        {
            #region 保存前校验

            var bill = _dataContext.Master;
            if (bill.StorageID == default(int))
            {
                MessageBox.Show("未指定盘点仓库");
                return;
            }
            if (bill.BrandID == default(int))
            {
                MessageBox.Show("未指定盘点品牌");
                return;
            }
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<DistributionProductShow>(gvDatas, bill.BrandID))
                return;
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            var details = _dataContext.Details = new List<BillStocktakeDetails>();
            //SysProcessView.UIHelper.TraverseGridViewData<DistributionProductForBrush>(gvDatas, p => { details.Add(new BillStocktakeDetails { ProductID = p.ProductID, Quantity = p.Quantity }); });
            _dataContext.TraverseGridDataItems(p => { details.Add(new BillStocktakeDetails { ProductID = p.ProductID, Quantity = p.Quantity }); });
            //if (!UIHelper.CheckDetailsWithBrand<BillStocktakeDetails>(details, bill.BrandID, gvDatas))
            //    return;
            if (details.Count == 0)
            {
                MessageBox.Show("没有需要保存的数据");
                return;
            }

            #endregion

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

        private void InitDataContext()
        {
            _dataContext.Init();
        }
    }
}
