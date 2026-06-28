using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>财务折旧：按期批量计提（直线法，折旧三铁律）+ 查看某期折旧流水。</summary>
    public class DepreciationPage : SimpleGridPage
    {
        private int _year = DateTime.Today.Year;
        private int _month = DateTime.Today.Month;
        private UILabel _period;

        public DepreciationPage()
        {
            AddToolButton("计提折旧", DoRun, 96);
            AddToolButton("选择月份", DoPick, 96);
            AddToolButton("刷新", ReloadData, 84);
            _period = new UILabel
            {
                AutoSize = true,
                Location = new Point(300, 16),
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(48, 48, 48)
            };
            Toolbar.Controls.Add(_period);
        }

        protected override void BuildColumns()
        {
            AddColumn("AssetNo", "资产编号", 110);
            AddColumn("AssetName", "资产名称", 140);
            AddColumn("MonthlyAmount", "本月折旧", 90);
            AddColumn("AccumulatedAmount", "累计折旧", 100);
            AddColumn("NetValue", "净值", 90);
            AddColumn("PeriodText", "计提期间", 90);
        }

        protected override void ReloadData()
        {
            if (_period != null) _period.Text = $"计提期间：{_year}-{_month:00}";
            Bind(DepreciationService.GetLogs(_year, _month));
        }

        private void DoPick()
        {
            var dlg = new EditDialog("选择计提期间", new List<EditField>
            {
                EditField.Int("year", "年份", _year, true),
                EditField.Int("month", "月份(1-12)", _month, true),
            });
            if (dlg.ShowDialog() != DialogResult.OK) return;
            int y = dlg.GetInt("year"), m = dlg.GetInt("month");
            if (m < 1 || m > 12) { UIMessageTip.ShowWarning("月份须在 1-12"); return; }
            _year = y; _month = m;
            ReloadData();
        }

        private void DoRun()
        {
            if (!UIMessageBox.ShowAsk($"确认对 {_year}-{_month:00} 计提折旧？\r\n（仅对该月之前购入、未折完的在册资产计提，重复执行不会重复入账）")) return;
            var r = DepreciationService.Run(_year, _month);
            ReloadData();
            UIMessageBox.ShowInfo(
                $"{r.Year}-{r.Month:00} 折旧计提完成\r\n\r\n" +
                $"本次新提：{r.Processed} 台\r\n本次折旧总额：{r.TotalAmount:N2}\r\n" +
                $"提至残值封顶（折完）：{r.Completed} 台\r\n已存在跳过（幂等）：{r.Skipped} 台");
        }
    }
}
