using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>借用 / 归还（短期，含预计归还与超时催还）。</summary>
    public class AssetBorrowPage : SimpleGridPage
    {
        private HashSet<int> _overdueIds = new HashSet<int>();

        public AssetBorrowPage()
        {
            AddToolButton("借用", DoBorrow, 84);
            AddToolButton("归还", DoGiveBack, 84);
            AddToolButton("生成催还通知", DoNotice, 130);
            AddToolButton("履历", DoHistory, 84);
            AddToolButton("刷新", ReloadData, 84);
            Grid.RowPrePaint += Grid_RowPrePaint;
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 130);
            AddColumn("StatusText", "状态", 60);
            AddColumn("EmpName", "借用人", 90);
            AddColumn("LocationName", "位置", 100);
        }

        protected override void ReloadData()
        {
            _overdueIds = new HashSet<int>(FlowService.GetOverdue().Select(o => o.AssetID));
            Bind(AssetService.GetAll());
        }

        // 超时借用行标红
        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= Grid.Rows.Count) return;
            if (Grid.Rows[e.RowIndex].DataBoundItem is AssetListItem it && _overdueIds.Contains(it.AssetID))
                Grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
        }

        private void DoBorrow()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("借用", new List<EditField>
            {
                EditField.Combo("emp", "借用人", AssetUsePage.EmployeeItems(), null, true),
                EditField.Date("due", "预计归还", DateTime.Today.AddDays(7)),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            FlowService.Borrow(cur.AssetID, dlg.GetInt("emp"), dlg.GetDate("due"));
            ReloadData();
        }

        private void DoGiveBack()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认归还资产「{cur.AssetName}」？")) return;
            FlowService.GiveBack(cur.AssetID);
            ReloadData();
        }

        private void DoNotice()
        {
            var overdue = FlowService.GetOverdue();
            if (overdue.Count == 0) { UIMessageBox.ShowSuccess("当前没有超时未还的借用资产。"); return; }
            var sb = new StringBuilder("【借用资产催还通知】\r\n");
            foreach (var o in overdue)
                sb.AppendLine($"· {o.EmpName}（{o.Mobile}）借用「{o.AssetName}（{o.AssetNo}）」，应归还于 {o.ExpectReturnDate:yyyy-MM-dd}，现已超期，请尽快归还。");
            try { Clipboard.SetText(sb.ToString()); } catch { }
            UIMessageBox.ShowInfo("已生成催还通知并复制到剪贴板：\r\n\r\n" + sb);
        }

        private void DoHistory()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            new AssetHistoryDialog(cur.AssetID, cur.AssetName).ShowDialog();
        }
    }
}
