using System.Collections.Generic;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>系统用户管理（仅 Admin）。新增/编辑/启用停用/重置密码。</summary>
    public class UserManagePage : SimpleGridPage
    {
        public UserManagePage()
        {
            AddToolButton("新增", DoAdd, 84);
            AddToolButton("编辑", DoEdit, 84);
            AddToolButton("启用/停用", DoToggle, 96);
            AddToolButton("重置密码", DoReset, 96);
            AddToolButton("刷新", ReloadData, 84);
        }

        protected override void BuildColumns()
        {
            AddColumn("LoginName", "登录名", 110);
            AddColumn("RealName", "姓名", 100);
            AddColumn("Role", "角色", 90);
            AddColumn("EmpName", "关联人员", 100);
            AddColumn("StatusText", "状态", 60);
            AddColumn("LastLoginTime", "最后登录", 140);
        }

        protected override void ReloadData() => Bind(UserService.GetAll());

        private static List<KeyValuePair<string, object>> RoleItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("Admin（管理员）", "Admin"),
            new KeyValuePair<string, object>("Operator（操作员）", "Operator"),
            new KeyValuePair<string, object>("Finance（财务）", "Finance"),
        };

        private void DoAdd()
        {
            var dlg = new EditDialog("新增用户", new List<EditField>
            {
                EditField.Text("login", "登录名", null, true),
                EditField.Text("real", "姓名", null),
                EditField.Combo("role", "角色", RoleItems(), "Operator", true),
                EditField.Combo("emp", "关联人员（可选）", AssetUsePage.EmployeeItems(), null),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            string login = dlg.GetString("login")?.Trim();
            if (string.IsNullOrEmpty(login)) { UIMessageTip.ShowWarning("登录名不能为空"); return; }
            if (UserService.LoginNameExists(login)) { UIMessageTip.ShowWarning("登录名已存在"); return; }
            UserService.Insert(login, dlg.GetString("real"), dlg.GetString("role"), dlg.GetNullableInt("emp"));
            ReloadData();
            UIMessageBox.ShowInfo($"用户已创建，初始密码为：{UserService.DefaultPassword}\r\n请通知其首次登录后修改密码。");
        }

        private void DoEdit()
        {
            var u = Selected<UserListItem>();
            if (u == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑用户 - " + u.LoginName, new List<EditField>
            {
                EditField.Text("real", "姓名", u.RealName),
                EditField.Combo("role", "角色", RoleItems(), u.Role, true),
                EditField.Combo("emp", "关联人员（可选）", AssetUsePage.EmployeeItems(), null),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            UserService.Update(u.UserID, dlg.GetString("real"), dlg.GetString("role"), dlg.GetNullableInt("emp"));
            ReloadData();
        }

        private void DoToggle()
        {
            var u = Selected<UserListItem>();
            if (u == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (u.UserID == Security.CurrentUser.UserID) { UIMessageTip.ShowWarning("不能停用当前登录账号"); return; }
            UserService.SetEnabled(u.UserID, !u.IsEnabled);
            ReloadData();
        }

        private void DoReset()
        {
            var u = Selected<UserListItem>();
            if (u == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认将「{u.LoginName}」密码重置为默认密码 {UserService.DefaultPassword}？")) return;
            AuthService.SetPassword(u.UserID, UserService.DefaultPassword);
            UIMessageBox.ShowSuccess("密码已重置为：" + UserService.DefaultPassword);
        }
    }
}
