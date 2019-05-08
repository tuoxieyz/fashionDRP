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
using Telerik.Windows.Controls;
using System.Collections;
using SysProcessModel;
using SysProcessViewModel;
using DistributionModel;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for DeliveryAsBuyout.xaml
    /// </summary>
    public partial class DeliveryAsBuyout : UserControl
    {
        BillDeliveryAsBuyoutVM _dataContext = new BillDeliveryAsBuyoutVM();

        public DeliveryAsBuyout()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            //gvDatas.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
            btnSave.Click += (sender, e) =>
            {
                btnSave.IsEnabled = false;
                this.btnSave_Click(sender, e);
                btnSave.IsEnabled = true;
            };
#if UniqueCode
            gvDatas.SetResourceReference(GridViewDataControl.RowDetailsTemplateProperty, "uniqueCodeDetailsTemplate");
#endif
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

        //private void ActDatasWhenBinding(IEnumerable items)
        //{
        //    var bill = _dataContext.Master;
        //    foreach (var item in items)
        //    {
        //        ProductForDelivery p = (ProductForDelivery)item;
        //        p.FloatPrice = this.GetToOrganizationFloatPrice(bill.ToOrganizationID, p.BYQID, p.Price);
        //    }
        //}

        //void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        ActDatasWhenBinding(e.NewItems);
        //    }
        //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset && gvDatas.Items.Count > 0)//微软的Reset害死人，参看 http://msdn.microsoft.com/zh-cn/library/system.collections.specialized.notifycollectionchangedaction.aspx
        //    {
        //        ActDatasWhenBinding(gvDatas.Items);
        //    }
        //}

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
            opresult = _dataContext.CheckFundSatisfyDelivery();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductForDelivery)btn.DataContext);
        }

        private void gvDatas_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            gvDatas.CalculateAggregates();
        }
    }
}
