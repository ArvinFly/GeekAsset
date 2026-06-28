using System;

namespace GeekAsset.Models
{
    /// <summary>资产状态文本工具。</summary>
    public static class AssetStatus
    {
        public static string Text(byte s)
        {
            switch (s)
            {
                case 1: return "闲置";
                case 2: return "在用";
                case 3: return "报修";
                case 4: return "借出";
                case 5: return "报废";
                default: return "";
            }
        }
    }

    /// <summary>资产黄金卡片（Asset_Info 主表，完整字段）。</summary>
    public class AssetInfo
    {
        public int AssetID { get; set; }
        public string AssetNo { get; set; }
        public string AssetName { get; set; }
        public int? CategoryID { get; set; }
        public string Model { get; set; }
        public byte Status { get; set; } = 1;          // 默认闲置
        public int? LocationID { get; set; }
        public int? CurrentEmpID { get; set; }

        // 财务与维保
        public int? SupplierID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? WarrantyExpireDate { get; set; }

        // IT 专属
        public string SN { get; set; }
        public string MacAddress { get; set; }
        public string IPAddress { get; set; }
        public byte? IPType { get; set; }              // 1固定 2DHCP
        public string Original_Spec { get; set; }
        public string Current_Spec { get; set; }

        // 折旧/报废
        public decimal? ResidualValue { get; set; }
        public bool IsDepreciated { get; set; }
        public DateTime? ScrapDate { get; set; }
        public string ScrapReason { get; set; }

        public string Remark { get; set; }
        public int? CreateBy { get; set; }
    }

    /// <summary>资产台账列表项（含联表展示名称）。</summary>
    public class AssetListItem
    {
        public int AssetID { get; set; }
        public string AssetNo { get; set; }
        public string AssetName { get; set; }
        public string CategoryName { get; set; }
        public string Model { get; set; }
        public byte Status { get; set; }
        public string LocationName { get; set; }
        public string EmpName { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpireDate { get; set; }
        public string SN { get; set; }

        public string StatusText => AssetStatus.Text(Status);
    }
}
