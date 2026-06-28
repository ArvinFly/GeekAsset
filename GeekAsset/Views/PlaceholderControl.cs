using System.Drawing;
using System.Windows.Forms;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>
    /// 占位内容页（Phase 1 框架期使用）。各业务模块后续以同样方式作为 UserControl 加载进主区域。
    /// </summary>
    public class PlaceholderControl : UserControl
    {
        public PlaceholderControl(string moduleTitle)
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            var lbl = new UILabel
            {
                Dock = DockStyle.Fill,
                Text = moduleTitle + "\r\n\r\n（功能建设中…）",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微软雅黑", 16F),
                ForeColor = Color.FromArgb(110, 110, 110)
            };
            Controls.Add(lbl);
        }
    }
}
