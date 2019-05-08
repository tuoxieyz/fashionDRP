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
using System.Collections;
using SysProcessViewModel;
using SysProcessModel;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Bill
{
    /// <summary>
    /// Interaction logic for GoodReturn.xaml
    /// </summary>
    public partial class GoodReturn : UserControl
    {
        BillGoodReturnVM _dataContext = new BillGoodReturnVM();

        public GoodReturn()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
            this.btnSave.Click += (sender, e) =>
            {
                btnSave.IsEnabled = false;
                this.Save();
                btnSave.IsEnabled = true;
            };
#if UniqueCode
            gvDatas.SetResourceReference(GridViewDataControl.RowDetailsTemplateProperty, "uniqueCodeDetailsTemplate");
#endif
        }

        private void txtProductCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                SysProcessView.UIHelper.ProductCodeInput<BillGoodReturn, BillGoodReturnDetails, GoodReturnProductShow>(tb, _dataContext, this);
                gvDatas.CalculateAggregates();
            }
        }

        /// <summary>
        /// 获取发给本机构的发货价(即上级机构的上浮价)
        /// </summary>
        //private decimal GetDeliveryPrice(int byqID, decimal price)
        //{
        //    var oid = OrganizationListVM.CurrentOrganization.ParentID;
        //    var pf = _priceFloatCache.FirstOrDefault(o => o.OrganizationID == oid && o.BYQID == byqID);
        //    if (pf == null)
        //    {
        //        var temp = VMGlobal.SysProcessQuery.LinqOP.Search<OrganizationPriceFloat>(o => o.OrganizationID == oid && o.BYQID == byqID).ToList();
        //        if (temp != null && temp.Count > 0)
        //            pf = temp[0];
        //        else
        //            pf = new OrganizationPriceFloat { BYQID = byqID, OrganizationID = oid, FloatRate = 0, LastNumber = -1 };
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            _dataContext.DeleteItem((GoodReturnProductShow)btn.DataContext);
        }

        private void Save()
        {
            var opresult = _dataContext.ValidateWhenSave();
            if (!opresult.IsSucceed)
            {
                MessageBox.Show(opresult.Message);
                return;
            }
            if (!SysProcessView.UIHelper.CheckGridViewDataWithBrand<DistributionProductShow>(gvDatas, _dataContext.Master.BrandID))
                return;
            var result = _dataContext.Save();
            if (result.IsSucceed)
            {
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("保存失败\n失败原因:" + result.Message);
            }
        }
    }
}
