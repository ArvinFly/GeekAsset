using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>单台资产履历（流转 + 硬件变更时间线）。</summary>
    public class AssetHistoryDialog : UIForm
    {
        public AssetHistoryDialog(int assetId, string title)
        {
            Text = "资产履历 - " + title;
            ShowIcon = false;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 460);

            var grid = new UIDataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TimeText", HeaderText = "时间", FillWeight = 110 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Kind", HeaderText = "类别", FillWeight = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Action", HeaderText = "动作", FillWeight = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Detail", HeaderText = "明细", FillWeight = 200 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Operator", HeaderText = "经办人", FillWeight = 70 });

            grid.DataSource = FlowService.GetHistory(assetId);
            Controls.Add(grid);
            Padding = new Padding(0, 35, 0, 0);
        }
    }
}
