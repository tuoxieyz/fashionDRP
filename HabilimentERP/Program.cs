//*********************************************
// 公司名称：              
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-6-15 15:06:48
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;
using System.Diagnostics;
using Telerik.Windows.Controls;
using System.Windows.Controls;
using System.Threading;

namespace HabilimentERP
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
#if Release
            if (args != null && args.Length > 0)
            {
                if (args[0] != "IsUpdated")
                {
                    DialogParameters parameter = new DialogParameters
                    {
                        Content = new TextBlock() { Text = string.Join("", args), TextWrapping = TextWrapping.Wrap },
                        Theme = new Windows7Theme(),
                        Header = "注意",
                        OkButtonContent = "是",
                        CancelButtonContent = "否",
                        Closed = new EventHandler<WindowClosedEventArgs>(OnConfirmClosed)
                    };
                    RadWindow.Confirm(parameter);
                }
            }
            else
            {
                //UpdateReady.Update(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"AutoUpdate\UpdateOnline.exe", Process.GetCurrentProcess().MainModule.FileName.Replace(" ", "nbsp;"));
            }
#endif       

            HabilimentERP.App app = new HabilimentERP.App();
            LocalizationManager.Manager = new ZhCNLocalizationManager();
            //HabilimentERP.Properties.Settings.Default.DecryptConnectionString();
            app.InitializeComponent();
            app.Run();
        }

        private static void OnConfirmClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == false)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
