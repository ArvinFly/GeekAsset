using System;

namespace GeekAsset.Models
{
    /// <summary>流转日志原始行（联表取名称）。</summary>
    public class FlowHistoryRow
    {
        public DateTime Time { get; set; }
        public string ActionType { get; set; }
        public string FromEmp { get; set; }
        public string ToEmp { get; set; }
        public string FromLoc { get; set; }
        public string ToLoc { get; set; }
        public DateTime? ExpectReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public string Remark { get; set; }
        public string OperatorName { get; set; }
    }

    /// <summary>硬件/维保日志原始行。</summary>
    public class HwHistoryRow
    {
        public DateTime Time { get; set; }
        public string ChangeType { get; set; }
        public string PartName { get; set; }
        public string OldSpec { get; set; }
        public string NewSpec { get; set; }
        public decimal Cost { get; set; }
        public string HandlerName { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>履历时间线统一展示项。</summary>
    public class HistoryItem
    {
        public DateTime Time { get; set; }
        public string Kind { get; set; }      // 流转 / 硬件
        public string Action { get; set; }
        public string Detail { get; set; }
        public string Operator { get; set; }
        public string TimeText => Time.ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>超时未还借用项。</summary>
    public class OverdueBorrow
    {
        public int AssetID { get; set; }
        public string AssetNo { get; set; }
        public string AssetName { get; set; }
        public string EmpName { get; set; }
        public string Mobile { get; set; }
        public DateTime? ExpectReturnDate { get; set; }
    }
}
