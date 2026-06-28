using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>报废处置（生命周期终态，写流转日志并进履历时间线）。</summary>
    public class AssetScrapPage : SimpleGridPage
    {
        public AssetScrapPage()
        {
            AddToolButton("报废处置", DoScrap, 96);
            AddToolButton("履历", DoHistory, 84);
            AddToolButton("刷新", ReloadData, 84);
            Grid.RowPrePaint += Grid_RowPrePaint;
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 130);
            AddColumn("StatusText", "状态", 60);
            AddColumn("EmpName", "当前持有人", 90);
            AddColumn("LocationName", "位置", 110);
        }

        protected override void ReloadData() => Bind(AssetService.GetAll());

        // 已报废行置灰
        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= Grid.Rows.Count) return;
            if (Grid.Rows[e.RowIndex].DataBoundItem is AssetListItem it && it.Status == 5)
                Grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Gray;
        }

        private void DoScrap()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (cur.Status == 5) { UIMessageTip.ShowWarning("该资产已是报废状态"); return; }
            if (!UIMessageBox.ShowAsk($"确认报废资产「{cur.AssetName}」？报废后不可撤销。")) return;
            var dlg = new EditDialog("报废处置", new List<EditField>
            {
                EditField.Multiline("reason", "报废原因", "损毁/老旧/无法维修")
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            FlowService.Scrap(cur.AssetID, dlg.GetString("reason"));
            ReloadData();
        }

        private void DoHistory()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            new AssetHistoryDialog(cur.AssetID, cur.AssetName).ShowDialog();
        }
    }
}
