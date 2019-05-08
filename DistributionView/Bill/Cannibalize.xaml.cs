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
using Telerik.Windows.Controls;
using DomainLogicEncap;
using SysProcessViewModel;
using Kernel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for Cannibalize.xaml
    /// </summary>
    public partial class Cannibalize : UserControl
    {
        BillCannibalizeVM _dataContext = new BillCannibalizeVM();

        public Cannibalize()
        {
            this.DataContext = _dataContext;
            InitializeComponent();

            this.btnSave.Click += (sender, e) =>
            {
                btnSave.IsEnabled = false;
                this.Save();
                btnSave.IsEnabled = true;
            };

            bool isFirstLoad = true;
            this.Loaded += delegate
            {
                if (isFirstLoad)
                {
                    isFirstLoad = false;
                    ((RadComboBox)cbxStorage.Content).SelectionChanged += new SelectionChangedEventHandler(cbxStorageOut_SelectionChanged);
#if UniqueCode
                gvDatas.RowDetailsTemplate = (DataTemplate)FindResource("uniqueCodeDetailsTemplate");
#endif
                }
            };
        }

        void cbxStorageOut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var bill = _dataContext.Master;
            if (bill.StorageID != default(int))
            {
                List<int> pids = new List<int>();
                SysProcessView.UIHelper.TraverseGridViewData<ProductForCannibalize>(gvDatas, p => pids.Add(p.ProductID));
                var stocks = StockLogic.GetStockInStorage(bill.StorageID, productIDs: pids.ToArray());
                SysProcessView.UIHelper.TraverseGridViewData<ProductForCannibalize>(gvDatas, p =>
                {
                    var stock = stocks.Find(s => s.ProductID == p.ProductID);
                    p.OutStorageStock = (stock == null ? 0 : stock.Quantity);
                });
            }
        }

        private void txtProductCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                SysProcessView.UIHelper.ProductCodeInput<BillCannibalize, BillCannibalizeDetails, ProductForCannibalize>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductForCannibalize)btn.DataContext);
        }

        private void Save()
        {
            var opresult = _dataContext.ValidateWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                return;
            }
            var bill = _dataContext.Master;
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<ProductForCannibalize>(gvDatas, bill.BrandID))
                return;
            var result = _dataContext.Save();
            if (result.IsSucceed)
            {
                MessageBox.Show("保存成功");
                if (ckPrint.IsChecked.HasValue && ckPrint.IsChecked.Value)
                {
                    this.Print();
                    _dataContext.Init();
                }
            }
            else
            {
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }

        private void Print()
        {
            var entity = new CannibalizePrintEntity
            {
                CreateTime = _dataContext.Master.CreateTime,
                Remark = _dataContext.Master.Remark,
                BillCode = _dataContext.Master.Code,
                OuterName = OrganizationListVM.CurrentOrganization.Name,
                InnerName = _dataContext.OrganizationsToCannibalizeIn.First(o => o.ID == _dataContext.Master.ToOrganizationID).Name
            };
            entity.ProductCollection = _dataContext.GridDataItems.Select(o =>
            {
                return new CannibalizePrintProduct
                {
                    Quantity = o.Quantity,
                    ProductCode = o.ProductCode,
                    BrandName = VMGlobal.PoweredBrands.FirstOrEmpty(b => b.ID == o.BrandID).Name,
                    Price = o.Price
                };
            });
            try
            {
                View.Extension.UIHelper.Print("CannibalizePrintTemplate.xaml", entity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("打印单据出错:" + ex.Message);
            }
        }

        #region 辅助类

        private class CannibalizePrintEntity
        {
            public string BillCode { get; set; }
            public string OuterName { get; set; }
            public string InnerName { get; set; }
            public string Remark { get; set; }
            public DateTime CreateTime { get; set; }
            public string CreateTimeString
            {
                get
                {
                    return CreateTime.ToString("yyyy-MM-dd HH:mm");
                }
            }
            public IEnumerable<CannibalizePrintProduct> ProductCollection { get; set; }
        }

        private class CannibalizePrintProduct
        {
            public string ProductCode { get; set; }
            public int Quantity { get; set; }
            public string BrandName { get; set; }
            public decimal Price { get; set; }
        }

        #endregion
    }
}
