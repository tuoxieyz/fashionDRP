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
using DomainLogicEncap;
using DistributionModel;
using Telerik.Windows.Controls;
using System.Collections;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for MoveStock.xaml
    /// </summary>
    public partial class MoveStock : UserControl
    {
        BillStoreMoveVM _dataContext = new BillStoreMoveVM();

        public MoveStock()
        {
            var storages = StorageInfoVM.Storages;
            if (storages.Count == 1)
            {
                MessageBox.Show("当前机构只有一个仓库,移库单不可操作.");
                return;
            }
            
            this.DataContext = _dataContext;
            InitializeComponent();
            cbxStorageOut.ItemsSource = storages;
            cbxStorageIn.ItemsSource = storages;
            cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            if (VMGlobal.PoweredBrands.Count == 1)
                _dataContext.Master.BrandID = VMGlobal.PoweredBrands[0].ID;
            this.btnSave.Click += (sender, e) =>
            {
                btnSave.IsEnabled = false;
                this.Save();
                btnSave.IsEnabled = true;
            };
            gvDatas.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);

            bool isFirstLoad = true;
            this.Loaded += delegate
            {
                if (isFirstLoad)
                {                    
                    isFirstLoad = false;
                    //((TextBox)txtProductCode.Content).KeyDown += new KeyEventHandler(txtProductCode_KeyUp);
                    ((RadComboBox)cbxStorageOut.Content).SelectionChanged += new SelectionChangedEventHandler(cbxStorageOut_SelectionChanged);
                    ((RadComboBox)cbxStorageIn.Content).SelectionChanged += new SelectionChangedEventHandler(cbxStorageIn_SelectionChanged);
#if UniqueCode
                    gvDatas.RowDetailsTemplate = (DataTemplate)FindResource("uniqueCodeDetailsTemplate");
#endif
                }
            };
        }

        void cbxStorageOut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var bill = _dataContext.Master;
            if (bill.StorageIDOut == bill.StorageIDIn)
            {
                SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p => p.OutStorageStock = p.InStorageStock);
            }
            else if (bill.StorageIDOut != default(int))
            {
                List<int> pids = new List<int>();
                SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p => pids.Add(p.ProductID));
                var stocks = StockLogic.GetStockInStorage(bill.StorageIDOut, productIDs: pids.ToArray());
                SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p =>
                {
                    var stock = stocks.Find(s => s.ProductID == p.ProductID);
                    p.OutStorageStock = (stock == null ? 0 : stock.Quantity);
                });
            }
        }

        void cbxStorageIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var bill = _dataContext.Master;
            if (bill.StorageIDOut == bill.StorageIDIn)
            {
                SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p => p.InStorageStock = p.OutStorageStock);
            }
            else if(bill.StorageIDIn != default(int))
            {
                List<int> pids = new List<int>();
                SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p => pids.Add(p.ProductID));
                var stocks = StockLogic.GetStockInStorage(bill.StorageIDIn, productIDs: pids.ToArray());
                SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p =>
                {
                    var stock = stocks.Find(s => s.ProductID == p.ProductID);
                    p.InStorageStock = (stock == null ? 0 : stock.Quantity);
                });
            }
        }

        private void TraverseDatas(IEnumerable items, Action<ProductForStoreMove> action)
        {
            foreach (var item in items)
            {
                ProductForStoreMove p = (ProductForStoreMove)item;
                action(p);
            }
        }

        private void ActDatasWhenBinding(IEnumerable items)
        {
            var bill = _dataContext.Master;
            List<int> pids = new List<int>();
            TraverseDatas(items, p => pids.Add(p.ProductID));
            if (bill.StorageIDOut != default(int))
            {
                var stocks = StockLogic.GetStockInStorage(bill.StorageIDOut, productIDs: pids.ToArray());
                TraverseDatas(items, p =>
                {
                    var stock = stocks.Find(s => s.ProductID == p.ProductID);
                    p.OutStorageStock = (stock == null ? 0 : stock.Quantity);
                });
            }
            if (bill.StorageIDIn != default(int))
            {
                if (bill.StorageIDOut == bill.StorageIDIn)//出入仓库一样就直接赋值
                {
                    TraverseDatas(items, p => p.InStorageStock = p.OutStorageStock);                    
                }
                else
                {
                    var stocks = StockLogic.GetStockInStorage(bill.StorageIDIn, productIDs: pids.ToArray());
                    TraverseDatas(items, p =>
                    {
                        var stock = stocks.Find(s => s.ProductID == p.ProductID);
                        p.InStorageStock = (stock == null ? 0 : stock.Quantity);
                    });
                }
            }
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                ActDatasWhenBinding(e.NewItems);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset && gvDatas.Items.Count > 0)//微软的Reset害死人，参看 http://msdn.microsoft.com/zh-cn/library/system.collections.specialized.notifycollectionchangedaction.aspx
            {
                ActDatasWhenBinding(gvDatas.Items);
            }
        }

        private void txtProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SysProcessView.UIHelper.ProductCodeInput<BillStoreMove, BillStoreMoveDetails, ProductForStoreMove>(txtProductCode, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductForStoreMove)btn.DataContext);
        }

        private void Save()
        {
            #region 保存前校验
            var opresult = _dataContext.ValidateWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                return;
            }
            var bill = _dataContext.Master;            
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<ProductForStoreMove>(gvDatas, bill.BrandID))
                return;
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            var details = _dataContext.Details = new List<BillStoreMoveDetails>();
            SysProcessView.UIHelper.TraverseGridViewData<ProductForStoreMove>(gvDatas, p => { details.Add(new BillStoreMoveDetails { ProductID = p.ProductID, Quantity = p.Quantity }); });
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
