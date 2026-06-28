using System;
using System.Collections.Generic;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>折旧规则配置（按资产分类设置预计年限与残值率）。</summary>
    public class DeprecRulePage : BaseListPage
    {
        private List<KeyValuePair<string, object>> CategoryItems()
        {
            var items = new List<KeyValuePair<string, object>>();
            foreach (var cat in CategoryService.GetAll())
                items.Add(new KeyValuePair<string, object>(cat.CategoryName, cat.CategoryID));
            return items;
        }

        protected override void BuildColumns()
        {
            AddColumn("CategoryName", "资产分类", 160);
            AddColumn("UsefulLifeYears", "预计年限(年)", 110);
            AddColumn("ResidualRateText", "残值率", 90);
        }

        protected override void ReloadData() => Bind(DeprecRuleService.GetAll());

        private List<EditField> Fields(BaseDepreciationRule m) => new List<EditField>
        {
            EditField.Combo("cat", "资产分类", CategoryItems(), m?.CategoryID, true),
            EditField.Int("years", "预计年限(年)", m?.UsefulLifeYears ?? 3, true),
            EditField.Decimal("rate", "残值率(%)", m != null ? m.ResidualRate * 100m : 5m, true),
        };

        protected override void DoAdd()
        {
            var dlg = new EditDialog("新增折旧规则", Fields(null));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            int catId = dlg.GetInt("cat");
            if (DeprecRuleService.ExistsForCategory(catId, 0)) { UIMessageBox.ShowWarning("该分类已存在折旧规则，请直接编辑。"); return; }
            DeprecRuleService.Insert(new BaseDepreciationRule
            {
                CategoryID = catId,
                UsefulLifeYears = dlg.GetInt("years"),
                ResidualRate = dlg.GetDecimal("rate") / 100m
            });
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<BaseDepreciationRule>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑折旧规则", Fields(cur));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            int catId = dlg.GetInt("cat");
            if (DeprecRuleService.ExistsForCategory(catId, cur.RuleID)) { UIMessageBox.ShowWarning("该分类已存在折旧规则。"); return; }
            cur.CategoryID = catId;
            cur.UsefulLifeYears = dlg.GetInt("years");
            cur.ResidualRate = dlg.GetDecimal("rate") / 100m;
            DeprecRuleService.Update(cur);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<BaseDepreciationRule>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除「{cur.CategoryName}」的折旧规则？")) return;
            DeprecRuleService.Delete(cur.RuleID);
            ReloadData();
        }
    }
}
