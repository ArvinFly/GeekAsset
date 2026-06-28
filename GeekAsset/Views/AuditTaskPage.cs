using System.Collections.Generic;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>扫码盘点：盘点单列表（新建 / 开始盘点 / 报告 / 结单 / 删除）。</summary>
    public class AuditTaskPage : SimpleGridPage
    {
        public AuditTaskPage()
        {
            AddToolButton("新建盘点单", DoCreate, 110);
            AddToolButton("开始盘点", DoRun, 96);
            AddToolButton("盘点报告", DoReport, 96);
            AddToolButton("完成结单", DoFinish, 96);
            AddToolButton("删除", DoDelete, 84);
            AddToolButton("刷新", ReloadData, 84);
        }

        protected override void BuildColumns()
        {
            AddColumn("TaskName", "盘点单名称", 200);
            AddColumn("ScopeText", "范围", 150);
            AddColumn("StatusText", "状态", 70);
            AddColumn("ProgressText", "进度(已盘/总)", 100);
            AddColumn("StartTime", "开始时间", 130);
        }

        protected override void ReloadData() => Bind(AuditService.GetTasks());

        private static List<KeyValuePair<string, object>> Items<T>(IEnumerable<T> src, System.Func<T, string> name, System.Func<T, object> id)
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var x in src) list.Add(new KeyValuePair<string, object>(name(x), id(x)));
            return list;
        }

        private void DoCreate()
        {
            var dlg = new EditDialog("新建盘点单", new List<EditField>
            {
                EditField.Text("name", "盘点单名称", null, true),
                EditField.Combo("dept", "按部门（三选一）", Items(DeptService.GetAll(), d => d.DeptName, d => d.DeptID), null),
                EditField.Combo("loc", "按位置（三选一）", Items(LocationService.GetAll(), l => l.LocationName, l => l.LocationID), null),
                EditField.Combo("cat", "按分类（三选一）", Items(CategoryService.GetAll(), c => c.CategoryName, c => c.CategoryID), null),
                EditField.Multiline("remark", "备注", null),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;

            int? dept = dlg.GetNullableInt("dept"), loc = dlg.GetNullableInt("loc"), cat = dlg.GetNullableInt("cat");
            int chosen = (dept.HasValue ? 1 : 0) + (loc.HasValue ? 1 : 0) + (cat.HasValue ? 1 : 0);
            if (chosen != 1) { UIMessageTip.ShowWarning("请在 部门/位置/分类 中恰好选择一种盘点范围"); return; }

            byte scopeType = dept.HasValue ? (byte)1 : loc.HasValue ? (byte)2 : (byte)3;
            int? scopeRef = dept ?? loc ?? cat;
            int taskId = AuditService.CreateTask(dlg.GetString("name"), scopeType, scopeRef, dlg.GetString("remark"));
            ReloadData();

            if (UIMessageBox.ShowAsk("盘点单已创建，是否立即开始盘点？"))
                new AuditRunForm(taskId, dlg.GetString("name")).ShowDialog();
            ReloadData();
        }

        private void DoRun()
        {
            var t = Selected<AuditTaskItem>();
            if (t == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (t.Status == 2) { UIMessageTip.ShowWarning("该盘点单已结单，仅可查看报告"); return; }
            new AuditRunForm(t.TaskID, t.TaskName).ShowDialog();
            ReloadData();
        }

        private void DoReport()
        {
            var t = Selected<AuditTaskItem>();
            if (t == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var r = AuditService.GetReport(t.TaskID);
            UIMessageBox.ShowInfo(
                $"【{t.TaskName}】盘盈盘亏报告\r\n\r\n" +
                $"应盘总数：{r.Total}\r\n已盘：{r.Audited}　未盘：{r.Pending}\r\n\r\n" +
                $"正常：{r.Normal}\r\n盘盈：{r.Surplus}\r\n盘亏：{r.Deficit}\r\n信息不符：{r.Mismatch}");
        }

        private void DoFinish()
        {
            var t = Selected<AuditTaskItem>();
            if (t == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (t.Status == 2) { UIMessageTip.ShowWarning("该盘点单已结单"); return; }
            if (!UIMessageBox.ShowAsk($"确认完成「{t.TaskName}」？结单后未盘资产将计为盘亏，不可再盘。")) return;
            AuditService.FinishTask(t.TaskID);
            ReloadData();
        }

        private void DoDelete()
        {
            var t = Selected<AuditTaskItem>();
            if (t == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除盘点单「{t.TaskName}」及其全部明细？")) return;
            AuditService.DeleteTask(t.TaskID);
            ReloadData();
        }
    }
}
