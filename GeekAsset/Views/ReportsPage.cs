using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>报表看板：关键指标卡 + 按状态/分类分布。</summary>
    public class ReportsPage : UserControl
    {
        private readonly FlowLayoutPanel _cards;
        private readonly UIDataGridView _statusGrid, _categoryGrid;

        public ReportsPage()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            var toolbar = new UIPanel { Dock = DockStyle.Top, Height = 46, FillColor = Color.White, Text = "" };
            var refresh = new UIButton { Text = "刷新", Location = new Point(12, 7), Size = new Size(84, 32) };
            refresh.Click += (s, e) => LoadData();
            toolbar.Controls.Add(refresh);

            _cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(8, 6, 8, 6), BackColor = Color.White };

            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.White };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            split.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.Controls.Add(new UILabel { Text = "  按状态分布", Dock = DockStyle.Fill, Font = new Font("微软雅黑", 11F, FontStyle.Bold) }, 0, 0);
            split.Controls.Add(new UILabel { Text = "  按分类分布", Dock = DockStyle.Fill, Font = new Font("微软雅黑", 11F, FontStyle.Bold) }, 1, 0);
            _statusGrid = MakeGrid();
            _categoryGrid = MakeGrid();
            split.Controls.Add(_statusGrid, 0, 1);
            split.Controls.Add(_categoryGrid, 1, 1);

            Controls.Add(split);
            Controls.Add(_cards);
            Controls.Add(toolbar);

            Load += (s, e) => LoadData();
        }

        private static UIDataGridView MakeGrid()
        {
            var g = new UIDataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "名称", FillWeight = 120 });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Count", HeaderText = "数量", FillWeight = 60 });
            g.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Value", HeaderText = "原值合计", FillWeight = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            return g;
        }

        private UIPanel MakeCard(string title, string value, Color accent)
        {
            var p = new UIPanel { Size = new Size(168, 92), FillColor = Color.White, RectColor = Color.FromArgb(225, 225, 225), Text = "", Margin = new Padding(6) };
            p.Controls.Add(new UILabel { Text = title, Location = new Point(14, 12), AutoSize = true, ForeColor = Color.Gray, Font = new Font("微软雅黑", 10F) });
            p.Controls.Add(new UILabel { Text = value, Location = new Point(14, 38), AutoSize = true, ForeColor = accent, Font = new Font("微软雅黑", 20F, FontStyle.Bold) });
            return p;
        }

        private void LoadData()
        {
            var s = ReportService.GetStats();
            _cards.Controls.Clear();
            _cards.Controls.Add(MakeCard("资产总数", s.TotalAssets.ToString(), Color.FromArgb(33, 150, 243)));
            _cards.Controls.Add(MakeCard("资产原值合计", s.TotalValue.ToString("N0"), Color.FromArgb(33, 150, 243)));
            _cards.Controls.Add(MakeCard("在用", s.InUse.ToString(), Color.FromArgb(76, 175, 80)));
            _cards.Controls.Add(MakeCard("借出", s.Borrowed.ToString(), Color.FromArgb(255, 152, 0)));
            _cards.Controls.Add(MakeCard("维保30天预警", s.WarrantySoon.ToString(), Color.FromArgb(244, 67, 54)));
            _cards.Controls.Add(MakeCard("借用超期", s.OverdueBorrow.ToString(), Color.FromArgb(244, 67, 54)));
            _cards.Controls.Add(MakeCard("耗材低库存", s.LowStock.ToString(), Color.FromArgb(244, 67, 54)));

            _statusGrid.DataSource = ReportService.ByStatus();
            _categoryGrid.DataSource = ReportService.ByCategory();
        }
    }
}
