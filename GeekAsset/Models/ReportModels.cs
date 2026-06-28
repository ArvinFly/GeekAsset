namespace GeekAsset.Models
{
    /// <summary>统计明细行（按状态/按分类通用）。</summary>
    public class StatRow
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    /// <summary>维保即将到期行（工作台预警）。</summary>
    public class WarrantyWarnRow
    {
        public string AssetNo { get; set; }
        public string AssetName { get; set; }
        public System.DateTime? WarrantyExpireDate { get; set; }
        public string ExpireText => WarrantyExpireDate?.ToString("yyyy-MM-dd");
        public int DaysLeft => WarrantyExpireDate.HasValue
            ? (int)(WarrantyExpireDate.Value.Date - System.DateTime.Today).TotalDays : 0;
    }

    /// <summary>看板汇总指标。</summary>
    public class DashboardStats
    {
        public int TotalAssets { get; set; }
        public decimal TotalValue { get; set; }
        public int Idle { get; set; }
        public int InUse { get; set; }
        public int Repair { get; set; }
        public int Borrowed { get; set; }
        public int Scrapped { get; set; }
        public int WarrantySoon { get; set; }    // 30天内维保到期
        public int OverdueBorrow { get; set; }   // 借用超期
        public int LowStock { get; set; }        // 耗材低库存
    }
}
