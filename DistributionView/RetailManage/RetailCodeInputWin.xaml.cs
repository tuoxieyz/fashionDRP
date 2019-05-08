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
using System.Windows.Shapes;
using DistributionViewModel;
using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using ERPViewModelBasic;
using ERPModelBO;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for RetailCodeInputWin.xaml
    /// </summary>
    public partial class RetailCodeInputWin : Window 
    {
        internal event Action SetRetailVMEvent;

        private BaseBillRetailVM _retailContext = null;

        public RetailCodeInputWin(BaseBillRetailVM retailContext)
        {
            _retailContext = retailContext;
            InitializeComponent();
            this.Loaded += delegate { txtRetailCode.Focus(); };
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string code = txtRetailCode.Text.Trim();
            if (string.IsNullOrEmpty(txtRetailCode.Text))
            {
                this.Close();
            }
            else
            {
                var lp = VMGlobal.DistributionQuery.LinqOP;
                var retail = lp.Search<BillRetail>(o => o.Code == code).FirstOrDefault();
                if (retail == null)
                {
                    MessageBox.Show("不存在该零售单号,请检查.");
                    return;
                }
                else
                {
                    if (SetRetailVMEvent != null)
                    {
                        var vm = _retailContext;
                        vm.Master = retail;
                        if (vm.Master.VIPID != null && vm.Master.VIPID != default(int))
                        {
                            VIPBO vip = null;
                            try
                            {
                                vip = BillWebApiInvoker.Instance.Invoke<VIPBO, int[]>(VMGlobal.PoweredBrands.Select(o => o.ID).ToArray(), "BillRetail/GetVIPInfo?vid=" + vm.Master.VIPID);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                                return;
                            }
                        }
                        var details = lp.Search<BillRetailDetails>(o => o.BillID == retail.ID).ToList();
                        var pids = details.Select(o => o.ProductID).ToArray();
                        var products =lp.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
                        foreach (var d in details)
                        {
                            var product =
#if UniqueCode
                                data.Find(o => o.Product.ProductID == d.ProductID).Product;
#else
 products.Find(o => o.ProductID == d.ProductID);
#endif
                            var item = new ProductForRetail
                            {
                                Quantity = d.Quantity,
                                ProductCode = product.ProductCode,
                                ProductID = product.ProductID,
                                CutMoney = d.CutMoney,
                                Price = d.Price,
                                StyleCode = product.StyleCode,
                                BYQID = product.BYQID,
                                ColorID = product.ColorID,
                                SizeID = product.SizeID
                            };
                            item.ColorCode = VMGlobal.Colors.Find(o => o.ID == item.ColorID).Code;
                            item.SizeName = VMGlobal.Sizes.Find(o => o.ID == item.SizeID).Name;
                            item.SizeCode = VMGlobal.Sizes.Find(o => o.ID == item.SizeID).Code;
                            var byq = VMGlobal.BYQs.Find(o => o.ID == item.BYQID);
                            item.BrandID = byq.BrandID;
                            item.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == item.BrandID).Code;
                            vm.GridDataItems.Add(item);
                            item.Discount = d.Discount;//折扣在设置VIP和列表增加记录时会自动计算，为了保持历史折扣，将折扣设置放在设置VIP和列表增加记录之后
                        }
                        SetRetailVMEvent();
                    }
                    this.Close();
                }
            }
        }
    }
}
