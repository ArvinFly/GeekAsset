using System;

namespace GeekAsset.Models
{
    /// <summary>折旧流水展示行。</summary>
    public class DepreciationRow
    {
        public int AssetID { get; set; }
        public string AssetNo { get; set; }
        public string AssetName { get; set; }
        public int DeprecYear { get; set; }
        public int DeprecMonth { get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal AccumulatedAmount { get; set; }
        public decimal NetValue { get; set; }
        public DateTime GenerateTime { get; set; }

        public string PeriodText => $"{DeprecYear}-{DeprecMonth:00}";
    }

    /// <summary>批量计提结果汇总。</summary>
    public class DepreciationResult
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Processed { get; set; }    // 本次新提条数
        public int Skipped { get; set; }      // 已存在（幂等跳过）
        public int Completed { get; set; }    // 本次提至残值封顶（折完）
        public decimal TotalAmount { get; set; }
    }
}
