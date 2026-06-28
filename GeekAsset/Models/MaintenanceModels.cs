using System;

namespace GeekAsset.Models
{
    /// <summary>维修/维保登记输入（写 Asset_Hardware_Log）。</summary>
    public class HwLogEntry
    {
        public int AssetID { get; set; }
        public string ChangeType { get; set; }      // 换件 / 升级 / 维修
        public byte? RepairMethod { get; set; }     // 1=内部更换 2=原厂/第三方外修
        public string FaultDesc { get; set; }       // 故障现象
        public int? ReporterEmpID { get; set; }     // 报修人
        public DateTime? SendDate { get; set; }     // 送修日期
        public string OldSpec { get; set; }         // 变更前配置快照
        public string NewSpec { get; set; }         // 变更后配置快照
        public string PartName { get; set; }        // 更换配件
        public decimal Cost { get; set; }           // 费用
        public int? SupplierID { get; set; }        // 外修单位
        public string Remark { get; set; }
    }
}
