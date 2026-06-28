using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>资产台账：入库、编辑、报废、删除、Excel 导入导出。</summary>
    public class AssetListPage : BaseListPage
    {
        public AssetListPage()
        {
            AddToolButton("报废", DoScrap, 84);
            AddToolButton("打印标签", DoPrintLabel, 96);
            AddToolButton("导入Excel", DoImport, 110);
            AddToolButton("导出Excel", DoExport, 110);
            AddToolButton("下载模板", DoTemplate, 110);
        }

        private void DoPrintLabel()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            new LabelPrintDialog(cur).ShowDialog();
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 130);
            AddColumn("CategoryName", "分类", 90);
            AddColumn("StatusText", "状态", 60);
            AddColumn("EmpName", "领用人", 80);
            AddColumn("LocationName", "位置", 100);
            AddColumn("PurchasePrice", "原值", 80);
            AddColumn("WarrantyExpireDate", "维保到期", 100);
            AddColumn("SN", "SN", 110);
        }

        protected override void ReloadData() => Bind(AssetService.GetAll());

        protected override void DoAdd()
        {
            var model = new AssetInfo();
            var frm = new AssetEditForm(model, true);
            if (frm.ShowDialog() != DialogResult.OK) return;
            AssetService.Insert(model);
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var model = AssetService.GetById(cur.AssetID);
            if (model == null) { UIMessageTip.ShowError("资产不存在"); return; }
            var frm = new AssetEditForm(model, false);
            if (frm.ShowDialog() != DialogResult.OK) return;
            AssetService.Update(model);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除资产「{cur.AssetName}」？")) return;
            AssetService.SoftDelete(cur.AssetID);
            ReloadData();
        }

        private void DoScrap()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (cur.Status == 5) { UIMessageTip.ShowWarning("该资产已是报废状态"); return; }
            var dlg = new EditDialog("报废处置", new List<EditField>
            {
                EditField.Multiline("reason", "报废原因", "损毁/老旧/无法维修")
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            FlowService.Scrap(cur.AssetID, dlg.GetString("reason"));
            ReloadData();
        }

        private void DoImport()
        {
            using (var ofd = new OpenFileDialog { Filter = "Excel 文件|*.xlsx", Title = "选择资产导入文件" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                var (ok, errors) = AssetService.ImportExcel(ofd.FileName);
                ReloadData();
                if (errors.Count == 0)
                    UIMessageBox.ShowSuccess($"成功导入 {ok} 条资产。");
                else
                    UIMessageBox.ShowWarning($"导入完成：成功 {ok} 条，失败 {errors.Count} 条。\r\n" +
                        string.Join("\r\n", errors.GetRange(0, Math.Min(errors.Count, 15))));
            }
        }

        private void DoExport()
        {
            using (var sfd = new SaveFileDialog { Filter = "Excel 文件|*.xlsx", FileName = $"资产台账_{DateTime.Now:yyyyMMdd}.xlsx" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                AssetService.ExportExcel(sfd.FileName);
                UIMessageBox.ShowSuccess("导出完成：" + sfd.FileName);
            }
        }

        private void DoTemplate()
        {
            using (var sfd = new SaveFileDialog { Filter = "Excel 文件|*.xlsx", FileName = "资产导入模板.xlsx" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                AssetService.ExportTemplate(sfd.FileName);
                UIMessageBox.ShowSuccess("模板已生成：" + sfd.FileName);
            }
        }
    }
}
