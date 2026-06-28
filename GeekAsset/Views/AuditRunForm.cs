using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>盘点执行窗：扫码核销 + 手工勾选 + 实时盘盈盘亏。</summary>
    public class AuditRunForm : UIForm
    {
        private readonly int _taskId;
        private readonly UITextBox _scanBox;
        private readonly UIDataGridView _grid;
        private readonly UILabel _stat;

        public AuditRunForm(int taskId, string taskName)
        {
            _taskId = taskId;
            Text = "盘点执行 - " + taskName;
            ShowIcon = false;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(940, 560);
            Padding = new Padding(0, 35, 0, 0);

            var top = new UIPanel { Dock = DockStyle.Top, Height = 56, FillColor = Color.White, Text = "" };
            var lbl = new UILabel { Text = "扫码/输入资产编号：", AutoSize = true, Location = new Point(14, 18) };
            _scanBox = new UITextBox { Location = new Point(150, 13), Width = 240, Font = new Font("微软雅黑", 11F) };
            _scanBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoScan(); } };
            var btnScan = new UIButton { Text = "盘点", Location = new Point(400, 11), Size = new Size(72, 32) };
            btnScan.Click += (s, e) => DoScan();
            var btnCheck = new UIButton { Text = "勾选/校正", Location = new Point(480, 11), Size = new Size(96, 32) };
            btnCheck.Click += (s, e) => DoCheckOff();
            var btnReset = new UIButton { Text = "撤销盘点", Location = new Point(584, 11), Size = new Size(88, 32) };
            btnReset.Click += (s, e) => DoReset();
            var btnFinish = new UIButton { Text = "完成结单", Location = new Point(680, 11), Size = new Size(96, 32) };
            btnFinish.Click += (s, e) => DoFinish();
            top.Controls.AddRange(new Control[] { lbl, _scanBox, btnScan, btnCheck, btnReset, btnFinish });

            _stat = new UILabel { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(14, 0, 0, 0) };

            _grid = new UIDataGridView
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
            AddCol("AuditStatusText", "盘点", 50);
            AddCol("ResultText", "结果", 60);
            AddCol("AssetNo", "资产编号", 110);
            AddCol("AssetName", "资产名称", 130);
            AddCol("BookEmpName", "账面使用人", 90);
            AddCol("ActualEmpName", "实际使用人", 90);
            AddCol("BookLocName", "账面位置", 100);
            AddCol("ActualLocName", "实际位置", 100);
            AddCol("ScannedCode", "实扫条码", 100);
            AddCol("Remark", "备注", 110);
            _grid.RowPrePaint += Grid_RowPrePaint;

            Controls.Add(_grid);
            Controls.Add(_stat);
            Controls.Add(top);

            Load += (s, e) => { Reload(); _scanBox.Focus(); };
        }

        private void AddCol(string prop, string header, int weight)
            => _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, FillWeight = weight });

        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _grid.Rows.Count) return;
            if (!(_grid.Rows[e.RowIndex].DataBoundItem is AuditDetailItem it)) return;
            Color c = Color.Empty;
            if (it.AuditStatus == 1) c = Color.Gray;                 // 未盘
            else if (it.ResultType == 2) c = Color.DarkOrange;       // 盘盈
            else if (it.ResultType == 3) c = Color.Red;              // 盘亏
            else if (it.ResultType == 4) c = Color.OrangeRed;        // 信息不符
            else if (it.ResultType == 1) c = Color.Green;            // 正常
            if (c != Color.Empty) _grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = c;
        }

        private void Reload()
        {
            _grid.DataSource = null;
            _grid.DataSource = AuditService.GetDetails(_taskId);
            var r = AuditService.GetReport(_taskId);
            _stat.Text = $"应盘 {r.Total}　已盘 {r.Audited}　未盘 {r.Pending}　|　正常 {r.Normal}　盘盈 {r.Surplus}　盘亏 {r.Deficit}　信息不符 {r.Mismatch}";
        }

        private void DoScan()
        {
            string code = _scanBox.Text;
            if (string.IsNullOrWhiteSpace(code)) return;
            try
            {
                string msg = AuditService.Scan(_taskId, code);
                _scanBox.Clear();
                _scanBox.Focus();
                Reload();
                UIMessageTip.ShowOk(msg);
            }
            catch (System.Exception ex) { UIMessageBox.ShowError("盘点失败：" + ex.Message); }
        }

        private void DoCheckOff()
        {
            if (!(_grid.CurrentRow?.DataBoundItem is AuditDetailItem it)) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (it.AssetID == null) { UIMessageTip.ShowWarning("盘盈条码无账面记录，无需勾选"); return; }
            var dlg = new EditDialog("勾选/校正盘点", new List<EditField>
            {
                EditField.Combo("emp", "实际使用人（不填=与账面一致）", AssetUsePage.EmployeeItems(), null),
                EditField.Combo("loc", "实际位置（不填=与账面一致）", LocationItems(), null),
                EditField.Multiline("remark", "备注", it.Remark),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                AuditService.CheckOff(it.DetailID, dlg.GetNullableInt("emp"), dlg.GetNullableInt("loc"), dlg.GetString("remark"));
                Reload();
            }
            catch (System.Exception ex) { UIMessageBox.ShowError("操作失败：" + ex.Message); }
        }

        private void DoReset()
        {
            if (!(_grid.CurrentRow?.DataBoundItem is AuditDetailItem it)) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (it.AuditStatus == 1) { UIMessageTip.ShowWarning("该行尚未盘点"); return; }
            try { AuditService.Reset(it.DetailID); Reload(); }
            catch (System.Exception ex) { UIMessageBox.ShowError("操作失败：" + ex.Message); }
        }

        private void DoFinish()
        {
            if (!UIMessageBox.ShowAsk("确认完成结单？未盘资产将计为盘亏，关闭后不可再盘。")) return;
            try { AuditService.FinishTask(_taskId); Reload(); Close(); }
            catch (System.Exception ex) { UIMessageBox.ShowError("结单失败：" + ex.Message); }
        }

        private static List<KeyValuePair<string, object>> LocationItems()
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var l in LocationService.GetAll()) list.Add(new KeyValuePair<string, object>(l.LocationName, l.LocationID));
            return list;
        }
    }
}
