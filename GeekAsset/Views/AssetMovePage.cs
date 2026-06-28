using System.Collections.Generic;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>资产调拨（跨部门 / 跨物理位置流转）。</summary>
    public class AssetMovePage : SimpleGridPage
    {
        public AssetMovePage()
        {
            AddToolButton("调拨", DoMove, 84);
            AddToolButton("履历", DoHistory, 84);
            AddToolButton("刷新", ReloadData, 84);
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 130);
            AddColumn("StatusText", "状态", 60);
            AddColumn("LocationName", "当前位置", 110);
            AddColumn("EmpName", "领用人", 90);
        }

        protected override void ReloadData() => Bind(AssetService.GetAll());

        private static List<KeyValuePair<string, object>> LocationItems()
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var l in LocationService.GetAll()) list.Add(new KeyValuePair<string, object>(l.LocationName, l.LocationID));
            return list;
        }

        private void DoMove()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("资产调拨", new List<EditField>
            {
                EditField.Combo("loc", "调入位置", LocationItems(), null, true),
                EditField.Multiline("remark", "调拨说明", null),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            FlowService.Transfer(cur.AssetID, dlg.GetInt("loc"), dlg.GetString("remark"));
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
