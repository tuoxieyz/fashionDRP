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
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using DistributionViewModel;
using DomainLogicEncap;
using Telerik.Windows.Controls;
using DistributionModel;
using System.Transactions;
using SysProcessModel;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for StockUpdateWithStocktake.xaml
    /// </summary>
    public partial class StockUpdateWithStocktake : UserControl
    {
        private List<int> _refrenceStocktakeIDs;//汇总数据关联的盘点单ID集合
        private List<ProBrand> _brands = VMGlobal.PoweredBrands;

        public StockUpdateWithStocktake()
        {
            InitializeComponent();

            var storages = StorageInfoVM.Storages;
            cbxStorage.ItemsSource = storages;
            if (storages.Count == 1)
                cbxStorage.SelectedIndex = 0;

            ItemPropertyDefinition[] billConditions = new[] {
                new ItemPropertyDefinition { DisplayName = "盘点日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                new ItemPropertyDefinition { DisplayName = "盘点品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                new ItemPropertyDefinition { DisplayName = "盘点单编号", PropertyName = "Code", PropertyType = typeof(string)}};

            billFilter.ItemPropertyDefinitions.AddRange(billConditions);

            billFilter.FilterDescriptors.Add(new FilterDescriptor("CreateTime", FilterOperator.IsEqualTo, DateTime.Now.Date, false));
            billFilter.FilterDescriptors.Add(new FilterDescriptor("Code", FilterOperator.Contains, FilterDescriptor.UnsetValue, false));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cbxStorage.SelectedIndex == -1)
            {
                cbxStorage.Focus();
                MessageBox.Show("请选择盘点仓库");
                return;
            }
            var filters = new CompositeFilterDescriptorCollection();
            //filters.AddRange(billFilter.FilterDescriptors.ToList());
            filters.Add(billFilter.FilterDescriptors);
            int storageID = (int)cbxStorage.SelectedValue;
            filters.Add(new FilterDescriptor("StorageID", FilterOperator.IsEqualTo, cbxStorage.SelectedValue));
            filters.Add(new FilterDescriptor("Status", FilterOperator.IsEqualTo, false));

            var data = ReportDataContext.AggregateStocktakeForStockUpdate(filters, out _refrenceStocktakeIDs);
            var pids = data.Select(o => o.ProductID);
            var stocks = VMGlobal.DistributionQuery.LinqOP.Search<Stock>(o => o.StorageID == storageID && pids.Contains(o.ProductID)).ToList();
            var result = data.Select(o =>
            {
                var d = new StocktakeAggregationEntityForStockUpdate
                    {
                        BrandID = o.BrandID,
                        BrandCode = o.BrandCode,
                        ColorCode = o.ColorCode,
                        ProductCode = o.ProductCode,
                        ProductID = o.ProductID,
                        Quantity = o.Quantity,
                        SizeCode = o.SizeCode,
                        SizeName = o.SizeName,
                        StyleCode = o.StyleCode
                    };
                var stock = stocks.FirstOrDefault(s => s.ProductID == o.ProductID);
                d.StockQuantity = (stock == null ? 0 : stock.Quantity);
                return d;
            }).ToList();
            RadGridView1.ItemsSource = result;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            View.Extension.UIHelper.ExcelExport(RadGridView1);
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "BrandID":
                        cbx.ItemsSource = _brands;//这么设置在多个品牌条件筛选时不会出现同步问题，难道只有以Binding的方式设置才会同步？
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void InitDataContext()
        {
            RadGridView1.ItemsSource = null;
            _refrenceStocktakeIDs = null;
        }

        private void cbxStorage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitDataContext();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (cbxStorage.SelectedIndex == -1)
            {
                cbxStorage.Focus();
                MessageBox.Show("请选择盘点仓库");
                return;
            }
            if (!rbUpdateAll.IsChecked.Value && !rbUpdatePart.IsChecked.Value && !rbUpdateExact.IsChecked.Value)
            {
                MessageBox.Show("请选择更新方式.");
                return;
            }
            if (RadGridView1.ItemsSource == null || RadGridView1.Items.Count == 0)
            {
                var mbResult = MessageBox.Show("没有盘点数据,如果选择继续将产生如下影响,是否继续?\n全局更新将清空全部库存\n局部更新将清空局部库存\n精确更新将不影响库存", "注意", MessageBoxButton.YesNo);
                if (mbResult == MessageBoxResult.No)
                    return;
            }

            List<DistributionCommonBillVM<BillStocktakeContrast, BillStocktakeContrastDetails>> contrasts = new List<DistributionCommonBillVM<BillStocktakeContrast, BillStocktakeContrastDetails>>();
            var stocktakeDatas = RadGridView1.ItemsSource as IEnumerable<StocktakeAggregationEntityForStockUpdate>;
            if (stocktakeDatas == null)
                stocktakeDatas = new List<StocktakeAggregationEntityForStockUpdate>();
            if (rbUpdateAll.IsChecked.Value)//全局更新
            {
                int[] brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID).ToArray();//GetBrandIDsForUpdate(stocktakeDatas);
                foreach (int bid in brandIDs)
                {
                    var stocks = StockLogic.GetStockInStorage((int)cbxStorage.SelectedValue, bid);
                    var contrast = this.GenerateContrast(bid, stocks, stocktakeDatas.Where(o => o.BrandID == bid).ToList());
                    //contrast.SaveWithNoTran();//添加盈亏单
                    contrasts.Add(contrast);
                }
            }
            else if (rbUpdatePart.IsChecked.Value)//局部更新
            {
                if (string.IsNullOrWhiteSpace(txtStyleCodeForPartUpdate.Text))
                {
                    txtStyleCodeForPartUpdate.Focus();
                    MessageBox.Show("请输入局部更新条码区间,多个区间以逗号分隔.");
                    return;
                }
                var pcodes = txtStyleCodeForPartUpdate.Text.Trim().Split(',');
                pcodes = pcodes.Select(o => o.Trim()).ToArray();
                stocktakeDatas = stocktakeDatas.ToList().Where(StockLogic.GenerateOrElseConditionWithArray<StocktakeAggregationEntityForStockUpdate>("ProductCode", "StartsWith", pcodes).Compile());
                int[] brandIDs = GetBrandIDsForUpdate(stocktakeDatas);
                foreach (int bid in brandIDs)
                {
                    var stocks = StockLogic.GetStockInStorage((int)cbxStorage.SelectedValue, bid, pcodes);
                    var contrast = this.GenerateContrast(bid, stocks, stocktakeDatas.Where(o=>o.BrandID == bid).ToList());
                    contrasts.Add(contrast);
                }
            }
            else if (rbUpdateExact.IsChecked.Value)//精确更新
            {
                if (stocktakeDatas.Count() == 0)
                    return;
                int[] brandIDs = GetBrandIDsForUpdate(stocktakeDatas);
                foreach (int bid in brandIDs)
                {
                    var stocks = StockLogic.GetStockInStorage((int)cbxStorage.SelectedValue, bid, stocktakeDatas.Where(o => o.BrandID == bid).Select(o => o.ProductID).ToArray());
                    var contrast = this.GenerateContrast(bid, stocks, stocktakeDatas.Where(o => o.BrandID == bid).ToList());
                    contrasts.Add(contrast);
                }
            }
            UpdateStockWithContrast(contrasts);
        }

        private void UpdateStockWithContrast(List<DistributionCommonBillVM<BillStocktakeContrast, BillStocktakeContrastDetails>> contrasts)
        {
            contrasts.RemoveAll(o => o.Details.Count == 0);
            if (contrasts.Count == 0)
            {
                MessageBox.Show("没有产生盈亏数据,库存量保持不变.");
                return;
            }
            var lp = VMGlobal.DistributionQuery.LinqOP;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    foreach (var contrast in contrasts)
                    {
                        contrast.SaveWithNoTran();//保存盈亏单
                        var pids = contrast.Details.Select(o => o.ProductID);
                        var stocks = lp.Search<Stock>(o => o.StorageID == contrast.Master.StorageID && pids.Contains(o.ProductID)).ToList();
                        var newStocks = contrast.Details.Select(o =>
                        {
                            var stock = stocks.FirstOrDefault(s => s.ProductID == o.ProductID);
                            if (stock != null)
                                stock.Quantity = o.QuaStocktake;
                            else
                                stock = new Stock { Quantity = o.QuaStocktake, ProductID = o.ProductID, StorageID = contrast.Master.StorageID };
                            return stock;
                        });
                        lp.AddOrUpdate<Stock>(newStocks);//更新库存                    
                    }
                    if (_refrenceStocktakeIDs != null)
                    {
                        var stIDArray = _refrenceStocktakeIDs.ToArray();
                        var sts = lp.Search<BillStocktake>(o => stIDArray.Contains(o.ID)).ToList();
                        sts.ForEach(o => o.Status = true);
                        lp.Update<BillStocktake>(sts);
                    }
                    scope.Complete();
                    InitDataContext();
                    MessageBox.Show("库存更新成功.");
                }
                catch (Exception e)
                {
                    MessageBox.Show("库存更新失败,失败原因:" + e.Message);
                }
            }
        }

        /// <summary>
        /// 根据盘点数据获取待更新的品牌ID集合
        /// </summary>
        private int[] GetBrandIDsForUpdate(IEnumerable<StocktakeAggregationEntityForStockUpdate> stocktakeDatas)
        {
            if (stocktakeDatas == null || stocktakeDatas.Count() == 0)
                return _brands.Select(o => o.ID).ToArray();
            else
                return stocktakeDatas.Select(o => o.BrandID).Distinct().ToArray();
        }

        /// <summary>
        /// 生成盈亏单
        /// </summary>
        /// <param name="brandID">一个仓库可能对应多个品牌，针对每个品牌生成各自的盈亏单</param>
        private DistributionCommonBillVM<BillStocktakeContrast, BillStocktakeContrastDetails> GenerateContrast(int brandID, IEnumerable<Stock> stocks, IEnumerable<StocktakeAggregationEntityForStockUpdate> stocktakeDatas)
        {
            var contrast = new DistributionCommonBillVM<BillStocktakeContrast, BillStocktakeContrastDetails>();
            var coMaster = contrast.Master;
            coMaster.Remark = "盘点盈亏";
            coMaster.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            coMaster.StorageID = (int)cbxStorage.SelectedValue;
            coMaster.BrandID = brandID;

            var pids = stocks.Select(o => o.ProductID).Union(stocktakeDatas.Select(o => o.ProductID)).ToArray();//获取库存和盘点的成品ID并集            
            List<BillStocktakeContrastDetails> coDetails = new List<BillStocktakeContrastDetails>();            
            Stock stock = null;
            StocktakeAggregationEntityForStockUpdate st = null;
            foreach (var pid in pids)
            {
                stock = stocks.FirstOrDefault(o => o.ProductID == pid);
                st = stocktakeDatas.FirstOrDefault(o => o.ProductID == pid);
                coDetails.Add(new BillStocktakeContrastDetails
                {
                    ProductID = pid,
                    QuaStockOrig = stock == null ? 0 : stock.Quantity,
                    QuaStocktake = st == null ? 0 : st.Quantity
                });
            }
            coDetails.ForEach(o => o.Quantity = o.QuaStocktake - o.QuaStockOrig);
            coDetails.RemoveAll(o => o.Quantity == 0);
            contrast.Details = coDetails;

            return contrast;
        }

        //private void rbUpdatePart_Checked(object sender, RoutedEventArgs e)
        //{
        //    txtStyleCodeForPartUpdate.SelectAll();
        //    txtStyleCodeForPartUpdate.Focus();            
        //}
    }
}
