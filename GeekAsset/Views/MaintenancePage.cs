using System.Collections.Generic;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>维修维保登记（换件/升级/维修，写硬件日志并同步当前配置；履历可查时间线）。</summary>
    public class MaintenancePage : SimpleGridPage
    {
        public MaintenancePage()
        {
            AddToolButton("维修维保登记", DoAdd, 120);
            AddToolButton("履历", DoHistory, 84);
            AddToolButton("刷新", ReloadData, 84);
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 130);
            AddColumn("StatusText", "状态", 60);
            AddColumn("Model", "规格型号", 120);
            AddColumn("EmpName", "使用人", 90);
            AddColumn("LocationName", "位置", 100);
        }

        protected override void ReloadData() => Bind(AssetService.GetAll());

        private static List<KeyValuePair<string, object>> SupplierItems()
        {
            var list = new List<KeyValuePair<string, object>>();
            foreach (var s in SupplierService.GetAll()) list.Add(new KeyValuePair<string, object>(s.SupplierName, s.SupplierID));
            return list;
        }

        private void DoAdd()
        {
            var cur = Selected<AssetListItem>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }

            var asset = AssetService.GetById(cur.AssetID);   // 取当前配置作变更前快照默认值
            var dlg = new EditDialog("维修维保登记 - " + cur.AssetName, new List<EditField>
            {
                EditField.Combo("type", "变更类型", new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("换件", "换件"),
                    new KeyValuePair<string, object>("升级", "升级"),
                    new KeyValuePair<string, object>("维修", "维修"),
                }, "维修", true),
                EditField.Combo("method", "维修方式", new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("内部更换", (byte)1),
                    new KeyValuePair<string, object>("原厂/第三方外修", (byte)2),
                }, null),
                EditField.Combo("reporter", "报修人", AssetUsePage.EmployeeItems(), null),
                EditField.Combo("supplier", "外修单位", SupplierItems(), null),
                EditField.Multiline("fault", "故障现象", null),
                EditField.Text("part", "更换配件", null),
                EditField.Multiline("oldspec", "变更前配置", asset?.Current_Spec),
                EditField.Multiline("newspec", "变更后配置", null),
                EditField.Decimal("cost", "费用", 0m),
                EditField.Date("send", "送修日期", System.DateTime.Today),
            });
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int? method = dlg.GetNullableInt("method");
            MaintenanceService.Add(new HwLogEntry
            {
                AssetID = cur.AssetID,
                ChangeType = dlg.GetString("type"),
                RepairMethod = method.HasValue ? (byte?)method.Value : null,
                FaultDesc = dlg.GetString("fault"),
                ReporterEmpID = dlg.GetNullableInt("reporter"),
                SendDate = dlg.GetDate("send"),
                OldSpec = dlg.GetString("oldspec"),
                NewSpec = dlg.GetString("newspec"),
                PartName = dlg.GetString("part"),
                Cost = dlg.GetDecimal("cost"),
                SupplierID = dlg.GetNullableInt("supplier"),
                Remark = null,
            });
            UIMessageTip.ShowOk("维修维保已登记");
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
