using DistributionModel;
using DistributionViewModel;
using Kernel;
using SysProcessViewModel;
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

namespace DistributionView.RetailManage
{
    /// <summary>
    /// RetailForSubordinate.xaml 的交互逻辑
    /// </summary>
    public partial class RetailForSubordinate : UserControl
    {
        private BaseBillRetailVM _dataContext = new RetailForSubordinateVM();

        public RetailForSubordinate()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            colDiscount.IsReadOnly = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((ProductForRetail)btn.DataContext);
        }

        private void txtProductCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var bill = _dataContext.Master;
                var tb = (TextBox)sender;
                SysProcessView.UIHelper.ProductCodeInput<BillRetail, BillRetailDetails, ProductForRetail>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        private void retailCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter == null)
                return;
            switch (e.Parameter.ToString())
            {
                case "Cash":
                    OPResult result = _dataContext.ValidateWhenCash();
                    if (!result.IsSucceed)
                    {
                        MessageBox.Show(result.Message);
                        return;
                    }
                    _dataContext.SetRetailData();
                    BindingExpression be = txtRemark.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateTarget();//备注可能更改,更新UI
                    CashWin wincash = new CashWin(_dataContext);
                    wincash.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                    wincash.CashScceedEvent += delegate
                    {
                        _dataContext.Init();
                        be.UpdateTarget();
                    };
                    wincash.ShowDialog();
                    break;
                case "CashCoupon":
                    List<int> brandIDs = new List<int>();
                    SysProcessView.UIHelper.TraverseGridViewData<DistributionProductShow>(gvDatas, p => { brandIDs.Add(p.BrandID); });
                    CashCouponWin couponwin = new CashCouponWin(brandIDs.Distinct().Select(o => VMGlobal.PoweredBrands.Find(b => b.ID == o)),
                        _dataContext.BeforeDiscountCoupon, _dataContext.AfterDiscountCoupon, _dataContext.CouponBrandIDs);
                    couponwin.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                    couponwin.CouponObtained += new Action<int, int, IEnumerable<int>>(win_CouponObtained);
                    couponwin.ShowDialog();
                    break;
                case "VIP":
                    break;
                case "Back":
                    RetailCodeInputWin win = new RetailCodeInputWin(_dataContext);
                    win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                    win.SetRetailVMEvent += () =>
                    {
                        foreach (var d in _dataContext.GridDataItems)
                        {
                            d.Quantity *= (-1);
                            d.CutMoney *= (-1);
                        }
                        _dataContext.Master.ReceiveTicket = 0;
                        _dataContext.Master.TicketKind = null;
                        _dataContext.Master.TicketMoney = 0;
                        //this.DataContext = _dataContext = vm;
                    };
                    win.ShowDialog();
                    break;
            }
        }

        void win_CouponObtained(int beforeDiscountCoupon, int afterDiscountCoupon, IEnumerable<int> brandIDs)
        {
            _dataContext.BeforeDiscountCoupon = beforeDiscountCoupon;
            _dataContext.AfterDiscountCoupon = afterDiscountCoupon;
            _dataContext.CouponBrandIDs = brandIDs;
        }
    }
}
