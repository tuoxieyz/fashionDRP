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
using SysProcessViewModel;
using ERPViewModelBasic;
using ERPModelBO;

namespace DistributionView.RetailManage
{
    /// <summary>
    /// Interaction logic for VIPInputWin.xaml
    /// </summary>
    public partial class VIPInputWin : Window
    {
        internal event Action<VIPBO> VIPObtained;
        internal event Action VIPClean;

        internal string Remark
        {
            set { tbRemark.Text = value; }
        }

        public VIPInputWin()
        {
            InitializeComponent();
            this.Loaded += delegate { txtVIPCode.Focus(); };
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string code = txtVIPCode.Text.Trim();
            if (string.IsNullOrEmpty(txtVIPCode.Text))
            {
                if (VIPClean != null)
                    VIPClean();
                this.Close();
            }
            else
            {
                VIPBO vip = null;
                try
                {
                    vip = BillWebApiInvoker.Instance.Invoke<VIPBO, int[]>(VMGlobal.PoweredBrands.Select(o => o.ID).ToArray(), "BillRetail/GetVIPInfo?vcode=" + code);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                if (vip == null)
                {
                    MessageBox.Show("VIP卡号不存在.");
                    return;
                }
                else
                {
                    if (VIPObtained != null)
                        VIPObtained(vip);
                    this.Close();
                }
            }
        }
    }
}
