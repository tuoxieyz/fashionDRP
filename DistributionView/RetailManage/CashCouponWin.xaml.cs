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
using DistributionModel;
using DistributionViewModel;
using SysProcessModel;
using SysProcessViewModel;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for CashCouponWin.xaml
    /// </summary>
    public partial class CashCouponWin : Window
    {
        internal event Action<int, int, IEnumerable<int>> CouponObtained;

        internal int BeforeDiscountCoupon
        {
            get { return (int)beforeDiscountCoupon.Value; }
        }

        internal int AfterDiscountCoupon
        {
            get { return (int)afterDiscountCoupon.Value; }
        }

        public CashCouponWin(IEnumerable<ProBrand> brands, int beforeCoupon, int afterCoupon,IEnumerable<int> appliedBrandIDs)
        {
            InitializeComponent();
            if (brands == null || brands.Count() == 0)
                brands = VMGlobal.PoweredBrands;
            lbBrand.ItemsSource = brands;
            beforeDiscountCoupon.Value = beforeCoupon;
            afterDiscountCoupon.Value = afterCoupon;
            if (appliedBrandIDs != null)
            {
                this.Loaded += delegate
                {
                    foreach (var brand in brands)
                    {
                        var item = lbBrand.ItemContainerGenerator.ContainerFromItem(brand) as ListBoxItem;
                        var ck = (CheckBox)item.Template.FindName("ckBrand", item);
                        if (appliedBrandIDs.Contains(brand.ID))
                            ck.IsChecked = true;
                        else
                            ck.IsChecked = false;
                    }
                };
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (CouponObtained != null)
            {
                var brands = (IEnumerable<ProBrand>)lbBrand.ItemsSource;
                List<int> brandIDs = new List<int>();
                foreach (var brand in brands)
                {
                    var item = lbBrand.ItemContainerGenerator.ContainerFromItem(brand) as ListBoxItem;
                    var ck = (CheckBox)item.Template.FindName("ckBrand", item);
                    if (ck.IsChecked.Value)
                        brandIDs.Add(brand.ID);
                }
                CouponObtained(BeforeDiscountCoupon, AfterDiscountCoupon, brandIDs);
            }
            this.Close();
        }
    }
}
