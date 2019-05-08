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
using Telerik.Windows.Controls;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrderForSubordinate.xaml
    /// </summary>
    public partial class OrderForSubordinate : UserControl
    {
        private ContractDiscountHelper _helper = new ContractDiscountHelper();
        DistributionCommonBillVM<BillOrder, BillOrderDetails> _dataContext = new DistributionCommonBillVM<BillOrder, BillOrderDetails>();

        public OrderForSubordinate()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            if (VMGlobal.PoweredBrands.Count == 1)
                _dataContext.Master.BrandID = VMGlobal.PoweredBrands[0].ID;
            gvDatas.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
        }

        private void ActDatasWhenBinding(IEnumerable items)
        {
            var bill = _dataContext.Master;
            foreach (var item in items)
            {
                DistributionProductShow p = (DistributionProductShow)item;
                p.Discount = _helper.GetDiscount(p.BYQID, bill.OrganizationID);
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
                if (bill.OrganizationID == default(int))
                {
                    MessageBox.Show("未指定订货机构");
                    return;
                }
                var tb = (TextBox)sender;
                SysProcessView.UIHelper.ProductCodeInput<BillOrder, BillOrderDetails, DistributionProductShow>(tb, _dataContext, this);
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
            if (bill.OrganizationID == default(int))
            {
                MessageBox.Show("未指定订货机构");
                return;
            }
            if (bill.BrandID == default(int))
            {
                MessageBox.Show("未指定订货品牌");
                return;
            }
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<DistributionProductShow>(gvDatas, bill.BrandID))
                return;
            bill.Status = (int)OrderStatusEnum.NotDelivered;
            var details = _dataContext.Details = new List<BillOrderDetails>();
            foreach (var item in gvDatas.Items)
            {
                var product = (DistributionProductShow)item;
                if (product.Quantity != 0)
                {
                    details.Add(new BillOrderDetails { ProductID = product.ProductID, Quantity = product.Quantity, QuaCancel = 0, QuaDelivered = 0, Status = (int)OrderStatusEnum.NotDelivered });
                }
            }
            if (details.Count == 0)
            {
                MessageBox.Show("没有需要保存的数据");
                return;
            }
            //if (!UIHelper.CheckDetailsWithBrand<BillOrderDetails>(details, bill.BrandID, gvDatas))
            //    return;
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
