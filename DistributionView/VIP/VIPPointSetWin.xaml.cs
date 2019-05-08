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

namespace DistributionView.VIP
{
    /// <summary>
    /// Interaction logic for VIPPointSetWin.xaml
    /// </summary>
    public partial class VIPPointSetWin : Window
    {
        private VIPCardBO _vip;

        public VIPPointSetWin(VIPCardBO vip)
        {
            _vip = vip;
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            VIPPointTrack track = this.DataContext as VIPPointTrack;
            if (track.Point == 0)
            {
                MessageBox.Show("增减积分不能为0.");
                return;
            }
            if (string.IsNullOrWhiteSpace(track.Remark))
            {
                MessageBox.Show("请填写备注信息方便以后查看.");
                return;
            }
            track.CreateTime = DateTime.Now;
            var lp = VMGlobal.DistributionQuery.LinqOP;            
            try
            {
                lp.Add<VIPPointTrack>(track);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败,失败原因:\n" + ex.Message);
                return;
            }
            _vip.PointTracks.Insert(0, track);
            MessageBox.Show("保存成功");
            this.Close();
        }
    }
}
