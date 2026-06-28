using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>耗材出入库流水。</summary>
    public class ConsumableLogDialog : UIForm
    {
        public ConsumableLogDialog(int consumableId, string name)
        {
            Text = "出入库流水 - " + name;
            ShowIcon = false;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(680, 420);
            Padding = new Padding(0, 35, 0, 0);

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
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ActionText", HeaderText = "类型", FillWeight = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "数量", FillWeight = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BalanceAfter", HeaderText = "操作后库存", FillWeight = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReceiverName", HeaderText = "领用人", FillWeight = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OperatorName", HeaderText = "经办人", FillWeight = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Remark", HeaderText = "备注", FillWeight = 110 });

            grid.DataSource = ConsumablesService.GetLogs(consumableId);
            Controls.Add(grid);
        }
    }
}
