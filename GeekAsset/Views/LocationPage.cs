using System.Collections.Generic;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>存放位置库管理。</summary>
    public class LocationPage : BaseListPage
    {
        protected override void BuildColumns()
        {
            AddColumn("LocationName", "位置名称", 120);
            AddColumn("Remark", "备注", 200);
        }

        protected override void ReloadData() => Bind(LocationService.GetAll());

        protected override void DoAdd()
        {
            var dlg = new EditDialog("新增存放位置", new List<EditField>
            {
                EditField.Text("name", "位置名称", null, true),
                EditField.Multiline("remark", "备注", null),
            });
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            LocationService.Insert(new BaseLocation { LocationName = dlg.GetString("name"), Remark = dlg.GetString("remark") });
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<BaseLocation>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑存放位置", new List<EditField>
            {
                EditField.Text("name", "位置名称", cur.LocationName, true),
                EditField.Multiline("remark", "备注", cur.Remark),
            });
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            cur.LocationName = dlg.GetString("name");
            cur.Remark = dlg.GetString("remark");
            LocationService.Update(cur);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<BaseLocation>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除位置「{cur.LocationName}」？")) return;
            LocationService.SoftDelete(cur.LocationID);
            ReloadData();
        }
    }
}
