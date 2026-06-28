using System;
using System.Windows.Forms;

namespace GeekAsset
{
    internal static class Program
    {
        /// <summary>应用程序的主入口点。先登录，成功后进入主窗体。</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 诊断开关：设环境变量 GEEKASSET_SKIP_LOGIN=1 可跳过登录直接进主窗体
            if (Environment.GetEnvironmentVariable("GEEKASSET_SKIP_LOGIN") == "1")
            {
                GeekAsset.Security.CurrentUser.Set(1, "admin", "系统管理员", "Admin");
            }
            else
            {
                using (var login = new LoginFrm())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                        return;   // 取消登录则退出
                }
            }

            Application.Run(new MainFrm());
        }
    }
}
