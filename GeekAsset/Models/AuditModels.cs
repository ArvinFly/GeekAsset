using System;

namespace GeekAsset.Models
{
    /// <summary>盘点任务列表项（含明细统计）。</summary>
    public class AuditTaskItem
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public byte ScopeType { get; set; }
        public string ScopeName { get; set; }       // 部门/位置/分类的名称
        public byte Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Remark { get; set; }
        public int Total { get; set; }               // 明细总数
        public int Audited { get; set; }             // 已盘数

        public string ScopeText
        {
            get
            {
                string t = ScopeType == 1 ? "部门" : ScopeType == 2 ? "位置" : ScopeType == 3 ? "分类" : "全部";
                return string.IsNullOrEmpty(ScopeName) ? t : t + "：" + ScopeName;
            }
        }
        public string StatusText => Status == 2 ? "已完成" : "进行中";
        public string ProgressText => $"{Audited}/{Total}";
    }

    /// <summary>盘点明细行（账面 vs 实际）。</summary>
    public class AuditDetailItem
    {
        public long DetailID { get; set; }
        public int? AssetID { get; set; }
        public string AssetNo { get; set; }
        public string AssetName { get; set; }
        public string ScannedCode { get; set; }
        public byte AuditStatus { get; set; }        // 1未盘 2已盘
        public byte? ResultType { get; set; }        // 1正常 2盘盈 3盘亏 4信息不符
        public string BookEmpName { get; set; }
        public string ActualEmpName { get; set; }
        public string BookLocName { get; set; }
        public string ActualLocName { get; set; }
        public DateTime? ScanTime { get; set; }
        public string Remark { get; set; }

        public string AuditStatusText => AuditStatus == 2 ? "已盘" : "未盘";
        public string ResultText
        {
            get
            {
                switch (ResultType)
                {
                    case 1: return "正常";
                    case 2: return "盘盈";
                    case 3: return "盘亏";
                    case 4: return "信息不符";
                    default: return "";
                }
            }
        }
    }

    /// <summary>盘盈盘亏汇总。</summary>
    public class AuditReport
    {
        public int Total { get; set; }
        public int Audited { get; set; }
        public int Normal { get; set; }
        public int Surplus { get; set; }     // 盘盈
        public int Deficit { get; set; }     // 盘亏
        public int Mismatch { get; set; }    // 信息不符
        public int Pending => Total - Audited;
    }
}
