//*********************************************
// 公司名称：              
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-6-18 10:03:42
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;

namespace UpdateOnline
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            UpdateOnline.App app = new UpdateOnline.App();
            string softPath = args[0].Replace("nbsp;", " ");
            UpdateHelper helper = new UpdateHelper(softPath);
            try
            {
                if (helper.Files == null || helper.Files.IsEmpty)
                    return;
            }
            catch (Exception ex)
            {                
                KillProcessesByName(System.IO.Path.GetFileNameWithoutExtension(softPath));
                Process.Start(softPath, "获取更新文件列表出错,信息:" + ex.Message + "\n是否继续运行当前版本?(注:当前版本不能保证运行正常.)");
                Process.GetCurrentProcess().Kill();
                return;
            }
            KillProcessesByName(System.IO.Path.GetFileNameWithoutExtension(softPath));
            var win = new MainWindow(helper);
            app.Run(win);
        }

        static void KillProcessesByName(string name)
        {
            var ps = Process.GetProcessesByName(name);
            foreach (var p in ps)
            {
                p.Kill();
            }
        }
    }
}
