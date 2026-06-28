using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>低值易耗品：台账维护 + 入库/领用 + 出入库流水。低于安全库存行标红。</summary>
    public class ConsumablesPage : SimpleGridPage
    {
        public ConsumablesPage()
        {
            AddToolButton("新增", DoAdd, 80);
            AddToolButton("编辑", DoEdit, 80);
            AddToolButton("删除", DoDelete, 80);
            AddToolButton("入库", DoStockIn, 80);
            AddToolButton("领用", DoIssue, 80);
            AddToolButton("流水", DoLogs, 80);
            AddToolButton("刷新", ReloadData, 80);
            Grid.RowPrePaint += Grid_RowPrePaint;
        }

        protected override void BuildColumns()
        {
            AddColumn("ConsumableName", "品名", 130);
            AddColumn("Spec", "规格", 110);
            AddColumn("Unit", "单位", 60);
            AddColumn("CategoryName", "类目", 90);
            AddColumn("StockText", "当前库存", 90);
            AddColumn("SafetyStock", "安全库存", 80);
        }

        protected override void ReloadData() => Bind(ConsumablesService.GetAll());

        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= Grid.Rows.Count) return;
            if (Grid.Rows[e.RowIndex].DataBoundItem is ConsumableInfo it && it.IsLow)
                Grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Red;
        }

        private void DoAdd()
        {
            var dlg = new EditDialog("新增耗材", Fields(null));
            if (dlg.ShowDialog() != DialogResult.OK) return;
            ConsumablesService.Insert(dlg.GetString("name"), dlg.GetString("spec"), dlg.GetString("unit"),
                dlg.GetString("cat"), dlg.GetInt("safety"));
            ReloadData();
        }

        private void DoEdit()
        {
            var it = Selected<ConsumableInfo>();
            if (it == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑耗材", Fields(it));
            if (dlg.ShowDialog() != DialogResult.OK) return;
            ConsumablesService.Update(it.ConsumableID, dlg.GetString("name"), dlg.GetString("spec"),
                dlg.GetString("unit"), dlg.GetString("cat"), dlg.GetInt("safety"));
            ReloadData();
        }

        private static List<EditField> Fields(ConsumableInfo it) => new List<EditField>
        {
            EditField.Text("name", "品名", it?.ConsumableName, true),
            EditField.Text("spec", "规格", it?.Spec),
            EditField.Text("unit", "单位", it?.Unit),
            EditField.Text("cat", "类目", it?.CategoryName),
            EditField.Int("safety", "安全库存", it?.SafetyStock ?? 0),
        };

        private void DoDelete()
        {
            var it = Selected<ConsumableInfo>();
            if (it == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除耗材「{it.ConsumableName}」？")) return;
            ConsumablesService.SoftDelete(it.ConsumableID);
            ReloadData();
        }

        private void DoStockIn()
        {
            var it = Selected<ConsumableInfo>();
            if (it == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("入库 - " + it.ConsumableName, new List<EditField>
            {
                EditField.Int("qty", "入库数量", 1, true),
                EditField.Multiline("remark", "备注", null),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            ConsumablesService.StockIn(it.ConsumableID, dlg.GetInt("qty"), dlg.GetString("remark"));
            ReloadData();
        }

        private void DoIssue()
        {
            var it = Selected<ConsumableInfo>();
            if (it == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("领用 - " + it.ConsumableName, new List<EditField>
            {
                EditField.Int("qty", "领用数量", 1, true),
                EditField.Combo("emp", "领用人", AssetUsePage.EmployeeItems(), null),
                EditField.Multiline("remark", "备注", null),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            ConsumablesService.Issue(it.ConsumableID, dlg.GetInt("qty"), dlg.GetNullableInt("emp"), dlg.GetString("remark"));
            ReloadData();
        }

        private void DoLogs()
        {
            var it = Selected<ConsumableInfo>();
            if (it == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            new ConsumableLogDialog(it.ConsumableID, it.ConsumableName).ShowDialog();
        }
    }
}
