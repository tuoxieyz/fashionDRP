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
using System.Collections;
using DomainLogicEncap;
using Telerik.Windows.Controls;
using SysProcessModel;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for DeliveryPackage.xaml
    /// </summary>
    public partial class DeliveryPackage : UserControl
    {
        //private ContractDiscountHelper _helper = new ContractDiscountHelper();
        BillDeliveryPackageVM _dataContext = new BillDeliveryPackageVM();

        public DeliveryPackage()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            gvDatas.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
            btnSave.Click += (sender, e) =>
            {
                btnSave.IsEnabled = false;
                this.btnSave_Click(sender, e);
                btnSave.IsEnabled = true;
            };
            bool isFirstLoad = true;
            this.Loaded += delegate
            {
                if (isFirstLoad)
                {
                    //KeyUp事件会导致，当校验未通过时,回车后焦点从弹出框回到文本框时重复执行校验逻辑
                    //((TextBox)txtProductCode.Content).KeyUp += new KeyEventHandler(txtProductCode_KeyUp);
                    //((TextBox)txtProductCode.Content).KeyDown += new KeyEventHandler(txtProductCode_KeyUp);
                    ckWriteDownOrder.Checked += new RoutedEventHandler(ckWriteDownOrder_Checked);
                    ckWriteDownOrder.Unchecked += new RoutedEventHandler(ckWriteDownOrder_Unchecked);
#if UniqueCode
                    gvDatas.RowDetailsTemplate = (DataTemplate)FindResource("uniqueCodeDetailsTemplate");
#endif
                    isFirstLoad = false;
                }
            };
        }

        //private List<OrganizationPriceFloat> _priceFloatCache = new List<OrganizationPriceFloat>();

        ///// <summary>
        ///// [根据发货价]获取下一机构的上浮价
        ///// </summary>
        ///// <param name="toOrganizationID">收货机构</param>
        ///// <param name="price">发货价</param>
        ///// <returns>上浮价</returns>
        //private decimal GetToOrganizationFloatPrice(int toOrganizationID, int byqID, decimal price)
        //{
        //    var pf = _priceFloatCache.FirstOrDefault(o => o.OrganizationID == toOrganizationID && o.BYQID == byqID);
        //    if (pf == null)
        //    {
        //        var temp = VMGlobal.SysProcessQuery.LinqOP.Search<OrganizationPriceFloat>(o => o.OrganizationID == toOrganizationID && o.BYQID == byqID).ToList();
        //        if (temp != null && temp.Count > 0)
        //            pf = temp[0];
        //        else
        //            pf = new OrganizationPriceFloat { BYQID = byqID, OrganizationID = toOrganizationID, FloatRate = 0, LastNumber = -1 };
        //        _priceFloatCache.Add(pf);
        //    }
        //    if (pf.LastNumber != -1)
        //    {
        //        price += pf.FloatRate * price * 0.01M;//上浮
        //        price *= 0.1M;
        //        price = decimal.Truncate(price) * 10 + pf.LastNumber;//尾数
        //    }
        //    return price;
        //}

        private void ActDatasWhenBinding(IEnumerable items)
        {
            var bill = _dataContext.Master;
            foreach (var item in items)
            {
                ProductForDelivery p = (ProductForDelivery)item;
                //p.Discount = _helper.GetDiscount(p.BYQID, bill.ToOrganizationID);
                //p.FloatPrice = this.GetToOrganizationFloatPrice(bill.ToOrganizationID, p.BYQID, p.Price);
                if (ckWriteDownOrder.IsChecked.Value)
                {
                    p.OrderQuantity = BillLogic.GetRemainOrderQuantity(bill.ToOrganizationID, p.ProductID);
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
                var bill = _dataContext.Master;
                if (bill.ToOrganizationID == default(int))
                {
                    MessageBox.Show("未指定收货机构");
                    return;
                }
                var tb = (TextBox)sender;
                SysProcessView.UIHelper.ProductCodeInput<BillDelivery, BillDeliveryDetails, ProductForDelivery>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void InitDataContext()
        {
            _dataContext.Init();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            #region 保存前校验

            var opresult = _dataContext.ValidateWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                return;
            }
            var bill = _dataContext.Master;
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<ProductForDelivery>(gvDatas, bill.BrandID))
                return;
            if (ckWriteDownOrder.IsChecked.Value)
            {
                bool flagStop = false;
                TraverseGridViewData(p =>
                {
                    if (flagStop = (p.Quantity > p.OrderQuantity))
                    {
                        MessageBox.Show("存在发货量大于订单量的条码,请检查!");
                        return;
                    }
                });
                if (flagStop) return;
            }
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            var details = _dataContext.Details = new List<BillDeliveryDetails>();
            TraverseGridViewData(p =>
            {
                details.Add(new BillDeliveryDetails { ProductID = p.ProductID, Quantity = p.Quantity, Discount = p.Discount, Price = p.Price });
            });
            //if (!UIHelper.CheckDetailsWithBrand<BillDeliveryDetails>(details, bill.BrandID, gvDatas))
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

        /// <summary>
        /// 遍历表格内数据
        /// </summary>
        private void TraverseGridViewData(Action<ProductForDelivery> action)
        {
            SysProcessView.UIHelper.TraverseGridViewData<ProductForDelivery>(gvDatas, action);
        }

        //private int[] GetBrandIDsInData()
        //{
        //    List<int> bids = new List<int>();
        //    TraverseGridViewData(p =>
        //    {
        //        if (!bids.Contains(p.BrandID))
        //            bids.Add(p.BrandID);
        //    });
        //    return bids.ToArray();
        //}

        private void ckWriteDownOrder_Unchecked(object sender, RoutedEventArgs e)
        {
            gvDatas.RowStyleSelector = null;
        }

        private void ckWriteDownOrder_Checked(object sender, RoutedEventArgs e)
        {
            if (gvDatas.RowStyleSelector == null)
            {
                DeliveryRowStyleSelector deliveryRowStyleSelector = (DeliveryRowStyleSelector)gridLayout.Resources["deliveryRowStyleSelector"];
                gvDatas.RowStyleSelector = deliveryRowStyleSelector;
            }
            var context = (BillDeliveryPackageVM)this.DataContext;
            var bill = context.Master;
            foreach (var item in gvDatas.Items)
            {
                ProductForDelivery p = (ProductForDelivery)item;
                p.OrderQuantity = BillLogic.GetRemainOrderQuantity(bill.ToOrganizationID, p.ProductID);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductForDelivery)btn.DataContext);
        }
    }
}
