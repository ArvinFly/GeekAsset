using System.Collections.Generic;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>领用 / 退还（长期绑定员工）。</summary>
    public class AssetUsePage : SimpleGridPage
    {
        public AssetUsePage()
        {
            AddToolButton("领用", DoLend, 84);
            AddToolButton("退还", DoReturn, 84);
            AddToolButton("履历", DoHistory, 84);
            AddToolButton("刷新", ReloadData, 84);
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 130);
            AddColumn("StatusText", "状态", 60);
            AddColumn("EmpName", "当前领用人", 90);
            AddColumn("LocationName", "位置", 110);
        }

        protected override void ReloadData() => Bind(AssetService.GetAll());

        internal static List<KeyValuePair<string, object>> EmployeeItems()
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var e in EmployeeService.GetAll()) list.Add(new KeyValuePair<string, object>(e.EmpName + "（" + e.EmpNo + "）", e.EmpID));
            return list;
        }

        private void DoLend()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("领用", new List<EditField> { EditField.Combo("emp", "领用人", EmployeeItems(), null, true) });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            FlowService.Lend(cur.AssetID, dlg.GetInt("emp"));
            ReloadData();
        }

        private void DoReturn()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认退还资产「{cur.AssetName}」？")) return;
            FlowService.Return(cur.AssetID);
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
