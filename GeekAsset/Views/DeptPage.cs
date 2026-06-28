using System;
using System.Collections.Generic;
using System.Linq;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>组织架构（公司-部门-小组）管理。</summary>
    public class DeptPage : BaseListPage
    {
        private List<SysDept> _data = new List<SysDept>();

        private static List<KeyValuePair<string, object>> LevelItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("公司", (byte)1),
            new KeyValuePair<string, object>("部门", (byte)2),
            new KeyValuePair<string, object>("小组", (byte)3),
        };

        protected override void BuildColumns()
        {
            AddColumn("DeptName", "名称", 140);
            AddColumn("ParentName", "上级", 140);
            AddColumn("SortOrder", "排序", 60);
        }

        protected override void ReloadData()
        {
            _data = DeptService.GetAll();
            var map = _data.ToDictionary(d => d.DeptID, d => d.DeptName);
            foreach (var d in _data)
                d.ParentName = d.ParentID.HasValue && map.ContainsKey(d.ParentID.Value) ? map[d.ParentID.Value] : "(顶级)";
            Bind(_data);
        }

        private List<KeyValuePair<string, object>> ParentItems(int excludeId)
        {
            var items = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("(顶级)", null) };
            foreach (var d in _data.Where(x => x.DeptID != excludeId))
                items.Add(new KeyValuePair<string, object>(d.DeptName, d.DeptID));
            return items;
        }

        private List<EditField> Fields(SysDept m) => new List<EditField>
        {
            EditField.Text("name", "名称", m?.DeptName, true),
            EditField.Combo("parent", "上级", ParentItems(m?.DeptID ?? 0), m?.ParentID, false),
            EditField.Combo("level", "层级", LevelItems(), m?.DeptLevel ?? (byte)2, true),
            EditField.Int("sort", "排序", m?.SortOrder ?? 0),
        };

        protected override void DoAdd()
        {
            var dlg = new EditDialog("新增部门", Fields(null));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            DeptService.Insert(new SysDept
            {
                DeptName = dlg.GetString("name"),
                ParentID = dlg.GetNullableInt("parent"),
                DeptLevel = Convert.ToByte(dlg.Values["level"]),
                SortOrder = dlg.GetInt("sort")
            });
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<SysDept>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑部门", Fields(cur));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            cur.DeptName = dlg.GetString("name");
            cur.ParentID = dlg.GetNullableInt("parent");
            cur.DeptLevel = Convert.ToByte(dlg.Values["level"]);
            cur.SortOrder = dlg.GetInt("sort");
            DeptService.Update(cur);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<SysDept>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (DeptService.HasChildren(cur.DeptID)) { UIMessageBox.ShowWarning("该部门下还有子部门，请先处理子级。"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除部门「{cur.DeptName}」？")) return;
            DeptService.SoftDelete(cur.DeptID);
            ReloadData();
        }
    }
}
