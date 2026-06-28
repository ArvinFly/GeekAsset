using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Security;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>修改当前登录用户的密码。</summary>
    public class ChangePasswordPage : UserControl
    {
        private readonly UITextBox _old, _new, _confirm;

        public ChangePasswordPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            var card = new UIPanel
            {
                Size = new Size(420, 320),
                Anchor = AnchorStyles.None,
                FillColor = Color.White,
                RectColor = Color.FromArgb(220, 220, 220),
                Text = ""
            };

            var title = new UILabel { Text = "修改密码", Location = new Point(30, 24), AutoSize = true, Font = new Font("微软雅黑", 14F, FontStyle.Bold) };
            var who = new UILabel { Text = $"当前账号：{CurrentUser.LoginName}", Location = new Point(30, 62), AutoSize = true, ForeColor = Color.Gray };

            _old = MakeBox(card, "原密码", 100);
            _new = MakeBox(card, "新密码", 160);
            _confirm = MakeBox(card, "确认新密码", 220);

            var btn = new UIButton { Text = "确认修改", Location = new Point(140, 268), Size = new Size(140, 36) };
            btn.Click += (s, e) => DoSubmit();

            card.Controls.AddRange(new Control[] { title, who, btn });
            Controls.Add(card);
            Resize += (s, e) => Center(card);
            Load += (s, e) => { Center(card); _old.Focus(); };
        }

        private void Center(Control card)
            => card.Location = new Point((Width - card.Width) / 2, System.Math.Max(20, (Height - card.Height) / 2));

        private UITextBox MakeBox(Control parent, string label, int y)
        {
            parent.Controls.Add(new UILabel { Text = label, Location = new Point(30, y), AutoSize = true });
            var box = new UITextBox { Location = new Point(140, y - 4), Width = 240, PasswordChar = '●' };
            parent.Controls.Add(box);
            return box;
        }

        private void DoSubmit()
        {
            string o = _old.Text, n = _new.Text, c = _confirm.Text;
            if (string.IsNullOrEmpty(o)) { UIMessageTip.ShowWarning("请输入原密码"); return; }
            if (string.IsNullOrEmpty(n) || n.Length < 6) { UIMessageTip.ShowWarning("新密码至少 6 位"); return; }
            if (n != c) { UIMessageTip.ShowWarning("两次输入的新密码不一致"); return; }
            if (n == o) { UIMessageTip.ShowWarning("新密码不能与原密码相同"); return; }

            if (AuthService.ChangePassword(CurrentUser.UserID, o, n))
            {
                _old.Clear(); _new.Clear(); _confirm.Clear();
                UIMessageBox.ShowSuccess("密码修改成功，下次登录请使用新密码。");
            }
            else UIMessageBox.ShowError("原密码不正确。");
        }
    }
}
