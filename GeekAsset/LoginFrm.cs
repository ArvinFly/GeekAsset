using System;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Security;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset
{
    public partial class LoginFrm : UIForm
    {
        public LoginFrm()
        {
            InitializeComponent();
            this.AcceptButton = btnLogin;   // 回车直接登录

            // 诊断：GEEKASSET_AUTO_LOGIN=1 时显示后自动登录，用于复现 登录→主窗体 路径
            if (Environment.GetEnvironmentVariable("GEEKASSET_AUTO_LOGIN") == "1")
            {
                this.Shown += (s, e) => btnLogin_Click(btnLogin, EventArgs.Empty);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string name = txtUser.Text.Trim();
            string pwd = txtPwd.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pwd))
            {
                UIMessageTip.ShowWarning("请输入登录名和密码");
                return;
            }

            try
            {
                SysUser user = AuthService.Login(name, pwd);
                if (user == null)
                {
                    UIMessageTip.ShowError("登录名或密码错误");
                    txtPwd.Text = string.Empty;
                    txtPwd.Focus();
                    return;
                }

                CurrentUser.Set(user.UserID, user.LoginName, user.RealName, user.Role);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError("登录失败：" + ex.Message);
            }
        }
    }
}
