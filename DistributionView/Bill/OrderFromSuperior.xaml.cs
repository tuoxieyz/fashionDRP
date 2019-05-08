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
using DomainLogicEncap;
using Telerik.Windows.Controls;
using SysProcessViewModel;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for OrderFormSuperior.xaml
    /// </summary>
    public partial class OrderFromSuperior : UserControl
    {
        private ContractDiscountHelper _helper = new ContractDiscountHelper();
        DistributionCommonBillVM<BillOrder, BillOrderDetails> _dataContext = new DistributionCommonBillVM<BillOrder, BillOrderDetails>();

        public OrderFromSuperior()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            if (VMGlobal.PoweredBrands.Count == 1)
                _dataContext.Master.BrandID = VMGlobal.PoweredBrands[0].ID;
            //AddingNewDataItem只能在内置的新增过程中触发
            //gvDatas.AddingNewDataItem += new EventHandler<Telerik.Windows.Controls.GridView.GridViewAddingNewEventArgs>(gvDatas_AddingNewDataItem);
            gvDatas.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Items_CollectionChanged);
            //bool isFirstLoad = true;
            //不知为何，运行时会加载两次，因此加了一个变量进行控制
            //this.Loaded += delegate
            //{
            //    if (isFirstLoad)
            //    {
            //        ((TextBox)txtProductCode.Content).KeyDown += new KeyEventHandler(txtProductCode_KeyUp);
            //        isFirstLoad = false;
            //    }
            //};
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    DistributionProductShow item = (DistributionProductShow)newItem;
                    item.Discount = _helper.GetDiscount(item.BYQID, VMGlobal.CurrentUser.OrganizationID);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset && gvDatas.Items.Count > 0)//微软的Reset害死人，参看 http://msdn.microsoft.com/zh-cn/library/system.collections.specialized.notifycollectionchangedaction.aspx
            {
                foreach (var newItem in gvDatas.Items)
                {
                    DistributionProductShow item = (DistributionProductShow)newItem;
                    item.Discount = _helper.GetDiscount(item.BYQID, VMGlobal.CurrentUser.OrganizationID);
                }
            }
        }

        private void txtProductCode_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SysProcessView.UIHelper.ProductCodeInput<BillOrder, BillOrderDetails, DistributionProductShow>(txtProductCode, _dataContext, this);
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
            if (bill.BrandID == default(int))
            {
                MessageBox.Show("未指定订货品牌");
                return;
            }
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<DistributionProductShow>(gvDatas, bill.BrandID))
                return;
            bill.Status = (int)OrderStatusEnum.NotDelivered;
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
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
