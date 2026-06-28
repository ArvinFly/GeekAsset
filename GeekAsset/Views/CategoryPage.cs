using System;
using System.Collections.Generic;
using System.Linq;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>资产分类树管理。</summary>
    public class CategoryPage : BaseListPage
    {
        private List<BaseAssetCategory> _data = new List<BaseAssetCategory>();

        private static List<KeyValuePair<string, object>> TypeItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("硬件资产", (byte)1),
            new KeyValuePair<string, object>("虚拟/软件资产", (byte)2),
        };

        protected override void BuildColumns()
        {
            AddColumn("CategoryName", "分类名称", 150);
            AddColumn("ParentName", "上级", 140);
            AddColumn("AssetTypeText", "资产类型", 130);
            AddColumn("SortOrder", "排序", 60);
        }

        protected override void ReloadData()
        {
            _data = CategoryService.GetAll();
            var map = _data.ToDictionary(d => d.CategoryID, d => d.CategoryName);
            foreach (var d in _data)
                d.ParentName = d.ParentID.HasValue && map.ContainsKey(d.ParentID.Value) ? map[d.ParentID.Value] : "(顶级)";
            Bind(_data);
        }

        private List<KeyValuePair<string, object>> ParentItems(int excludeId)
        {
            var items = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("(顶级)", null) };
            foreach (var d in _data.Where(x => x.CategoryID != excludeId))
                items.Add(new KeyValuePair<string, object>(d.CategoryName, d.CategoryID));
            return items;
        }

        private List<EditField> Fields(BaseAssetCategory m) => new List<EditField>
        {
            EditField.Text("name", "分类名称", m?.CategoryName, true),
            EditField.Combo("parent", "上级", ParentItems(m?.CategoryID ?? 0), m?.ParentID, false),
            EditField.Combo("type", "资产类型", TypeItems(), m?.AssetType ?? (byte)1, true),
            EditField.Int("sort", "排序", m?.SortOrder ?? 0),
        };

        protected override void DoAdd()
        {
            var dlg = new EditDialog("新增分类", Fields(null));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            CategoryService.Insert(new BaseAssetCategory
            {
                CategoryName = dlg.GetString("name"),
                ParentID = dlg.GetNullableInt("parent"),
                AssetType = Convert.ToByte(dlg.Values["type"]),
                SortOrder = dlg.GetInt("sort")
            });
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<BaseAssetCategory>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑分类", Fields(cur));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            cur.CategoryName = dlg.GetString("name");
            cur.ParentID = dlg.GetNullableInt("parent");
            cur.AssetType = Convert.ToByte(dlg.Values["type"]);
            cur.SortOrder = dlg.GetInt("sort");
            CategoryService.Update(cur);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<BaseAssetCategory>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (CategoryService.HasChildren(cur.CategoryID)) { UIMessageBox.ShowWarning("该分类下还有子分类，请先处理子级。"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除分类「{cur.CategoryName}」？")) return;
            CategoryService.SoftDelete(cur.CategoryID);
            ReloadData();
        }
    }
}
