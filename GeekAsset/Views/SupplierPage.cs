using System;
using System.Collections.Generic;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>往来单位（供应商/维保商）管理。</summary>
    public class SupplierPage : BaseListPage
    {
        private static List<KeyValuePair<string, object>> TypeItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("供应商", (byte)1),
            new KeyValuePair<string, object>("维保商", (byte)2),
            new KeyValuePair<string, object>("供应商+维保商", (byte)3),
        };

        protected override void BuildColumns()
        {
            AddColumn("SupplierName", "单位名称", 150);
            AddColumn("SupplierTypeText", "类型", 110);
            AddColumn("ContactPerson", "联系人", 90);
            AddColumn("Phone", "电话/热线", 120);
            AddColumn("PaymentTerm", "账期", 110);
            AddColumn("Remark", "备注", 130);
        }

        protected override void ReloadData() => Bind(SupplierService.GetAll());

        private List<EditField> Fields(BaseSupplier m) => new List<EditField>
        {
            EditField.Text("name", "单位名称", m?.SupplierName, true),
            EditField.Combo("type", "类型", TypeItems(), m?.SupplierType ?? (byte)1, true),
            EditField.Text("contact", "联系人", m?.ContactPerson),
            EditField.Text("phone", "电话/热线", m?.Phone),
            EditField.Text("term", "账期", m?.PaymentTerm),
            EditField.Multiline("remark", "备注", m?.Remark),
        };

        private void Apply(BaseSupplier m, EditDialog dlg)
        {
            m.SupplierName = dlg.GetString("name");
            m.SupplierType = Convert.ToByte(dlg.Values["type"]);
            m.ContactPerson = dlg.GetString("contact");
            m.Phone = dlg.GetString("phone");
            m.PaymentTerm = dlg.GetString("term");
            m.Remark = dlg.GetString("remark");
        }

        protected override void DoAdd()
        {
            var dlg = new EditDialog("新增往来单位", Fields(null));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            var m = new BaseSupplier();
            Apply(m, dlg);
            SupplierService.Insert(m);
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<BaseSupplier>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑往来单位", Fields(cur));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            Apply(cur, dlg);
            SupplierService.Update(cur);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<BaseSupplier>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除单位「{cur.SupplierName}」？")) return;
            SupplierService.SoftDelete(cur.SupplierID);
            ReloadData();
        }
    }
}
