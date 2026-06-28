using System;

namespace GeekAsset.Models
{
    /// <summary>低值易耗品（含库存）。</summary>
    public class ConsumableInfo
    {
        public int ConsumableID { get; set; }
        public string ConsumableName { get; set; }
        public string Spec { get; set; }
        public string Unit { get; set; }
        public string CategoryName { get; set; }
        public int Inventory { get; set; }
        public int SafetyStock { get; set; }

        /// <summary>低于安全库存预警。</summary>
        public bool IsLow => SafetyStock > 0 && Inventory <= SafetyStock;
        public string StockText => IsLow ? Inventory + "（偏低）" : Inventory.ToString();
    }

    /// <summary>耗材出入库流水行。</summary>
    public class ConsumableLogRow
    {
        public long LogID { get; set; }
        public byte ActionType { get; set; }       // 1入库 2领用
        public int Quantity { get; set; }
        public int BalanceAfter { get; set; }
        public string ReceiverName { get; set; }
        public string OperatorName { get; set; }
        public DateTime ActionTime { get; set; }
        public string Remark { get; set; }

        public string ActionText => ActionType == 1 ? "入库" : "领用";
        public string TimeText => ActionTime.ToString("yyyy-MM-dd HH:mm");
    }
}
