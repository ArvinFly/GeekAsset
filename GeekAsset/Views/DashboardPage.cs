using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GeekAsset.Security;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>工作台首页：欢迎 + 关键指标卡 + 待办预警（维保到期/借用超期/低库存）。</summary>
    public class DashboardPage : UserControl
    {
        private readonly UILabel _welcome;
        private readonly FlowLayoutPanel _cards;
        private readonly UIDataGridView _warranty, _overdue, _lowStock;

        public DashboardPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            var header = new UIPanel { Dock = DockStyle.Top, Height = 70, FillColor = Color.White, Text = "" };
            _welcome = new UILabel { Location = new Point(16, 12), AutoSize = true, Font = new Font("微软雅黑", 16F, FontStyle.Bold) };
            var sub = new UILabel { Location = new Point(18, 44), AutoSize = true, ForeColor = Color.Gray, Text = "GeekAsset 企业IT固定资产管理系统" };
            var refresh = new UIButton { Text = "刷新", Anchor = AnchorStyles.Top | AnchorStyles.Right, Size = new Size(80, 30) };
            refresh.Click += (s, e) => LoadData();
            header.Controls.AddRange(new Control[] { _welcome, sub, refresh });
            header.Resize += (s, e) => refresh.Location = new Point(header.Width - 96, 18);

            _cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 108, Padding = new Padding(8, 4, 8, 4), BackColor = Color.White };

            var grids = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, BackColor = Color.White };
            for (int i = 0; i < 3; i++) grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            grids.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            grids.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            grids.Controls.Add(Title("⏰ 维保 30 天内到期"), 0, 0);
            grids.Controls.Add(Title("⚠ 借用超期未还"), 1, 0);
            grids.Controls.Add(Title("📦 耗材低库存"), 2, 0);

            _warranty = Grid(("AssetNo", "编号", 90), ("AssetName", "名称", 120), ("ExpireText", "到期", 90));
            _overdue = Grid(("AssetName", "资产", 110), ("EmpName", "借用人", 80), ("ExpectReturnDate", "应还", 90));
            _lowStock = Grid(("ConsumableName", "耗材", 120), ("Inventory", "库存", 60), ("SafetyStock", "安全", 60));
            grids.Controls.Add(_warranty, 0, 1);
            grids.Controls.Add(_overdue, 1, 1);
            grids.Controls.Add(_lowStock, 2, 1);

            Controls.Add(grids);
            Controls.Add(_cards);
            Controls.Add(header);

            Load += (s, e) => LoadData();
        }

        private static UILabel Title(string text)
            => new UILabel { Text = "  " + text, Dock = DockStyle.Fill, Font = new Font("微软雅黑", 11F, FontStyle.Bold) };

        private static UIDataGridView Grid(params (string prop, string header, int weight)[] cols)
        {
            var g = new UIDataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(6, 0, 6, 6)
            };
            foreach (var c in cols)
                g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = c.prop, HeaderText = c.header, FillWeight = c.weight });
            return g;
        }

        private UIPanel Card(string title, string value, Color accent)
        {
            var p = new UIPanel { Size = new Size(176, 88), FillColor = Color.White, RectColor = Color.FromArgb(225, 225, 225), Text = "", Margin = new Padding(6) };
            p.Controls.Add(new UILabel { Text = title, Location = new Point(14, 10), AutoSize = true, ForeColor = Color.Gray, Font = new Font("微软雅黑", 10F) });
            p.Controls.Add(new UILabel { Text = value, Location = new Point(14, 34), AutoSize = true, ForeColor = accent, Font = new Font("微软雅黑", 20F, FontStyle.Bold) });
            return p;
        }

        private void LoadData()
        {
            int h = DateTime.Now.Hour;
            string greet = h < 6 ? "凌晨好" : h < 12 ? "上午好" : h < 14 ? "中午好" : h < 18 ? "下午好" : "晚上好";
            string name = string.IsNullOrEmpty(CurrentUser.RealName) ? CurrentUser.LoginName : CurrentUser.RealName;
            _welcome.Text = $"{greet}，{name}！";

            var s = ReportService.GetStats();
            _cards.Controls.Clear();
            _cards.Controls.Add(Card("资产总数", s.TotalAssets.ToString(), Color.FromArgb(33, 150, 243)));
            _cards.Controls.Add(Card("资产原值", s.TotalValue.ToString("N0"), Color.FromArgb(33, 150, 243)));
            _cards.Controls.Add(Card("在用", s.InUse.ToString(), Color.FromArgb(76, 175, 80)));
            _cards.Controls.Add(Card("借出", s.Borrowed.ToString(), Color.FromArgb(255, 152, 0)));
            _cards.Controls.Add(Card("维保预警", s.WarrantySoon.ToString(), Color.FromArgb(244, 67, 54)));
            _cards.Controls.Add(Card("借用超期", s.OverdueBorrow.ToString(), Color.FromArgb(244, 67, 54)));
            _cards.Controls.Add(Card("低库存", s.LowStock.ToString(), Color.FromArgb(244, 67, 54)));

            _warranty.DataSource = ReportService.WarrantySoonList();
            _overdue.DataSource = FlowService.GetOverdue();
            _lowStock.DataSource = ConsumablesService.GetAll().Where(x => x.IsLow).ToList();
        }
    }
}
