using System;

namespace GeekAsset.Models
{
    /// <summary>组织架构（Sys_Dept）。ParentName 为展示用，非数据库字段。</summary>
    public class SysDept
    {
        public int DeptID { get; set; }
        public int? ParentID { get; set; }
        public string DeptName { get; set; }
        public byte DeptLevel { get; set; }
        public int SortOrder { get; set; }
        public string ParentName { get; set; }
    }

    /// <summary>人员档案（Sys_Employee）。DeptName / StatusText 为展示用。</summary>
    public class SysEmployee
    {
        public int EmpID { get; set; }
        public string EmpNo { get; set; }
        public string EmpName { get; set; }
        public int? DeptID { get; set; }
        public string Mobile { get; set; }
        public byte Status { get; set; }          // 1在职 0离职
        public DateTime? LeaveDate { get; set; }
        public string DeptName { get; set; }
        public string StatusText => Status == 1 ? "在职" : "离职";
    }

    /// <summary>资产分类（Base_AssetCategory）。</summary>
    public class BaseAssetCategory
    {
        public int CategoryID { get; set; }
        public int? ParentID { get; set; }
        public string CategoryName { get; set; }
        public byte AssetType { get; set; }       // 1硬件 2虚拟/软件
        public int SortOrder { get; set; }
        public string ParentName { get; set; }
        public string AssetTypeText => AssetType == 1 ? "硬件资产" : "虚拟/软件资产";
    }

    /// <summary>存放位置（Base_Location）。</summary>
    public class BaseLocation
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>往来单位（Base_Supplier）。</summary>
    public class BaseSupplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public byte SupplierType { get; set; }    // 1供应商 2维保商 3两者
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string PaymentTerm { get; set; }
        public string Remark { get; set; }
        public string SupplierTypeText
        {
            get
            {
                switch (SupplierType)
                {
                    case 1: return "供应商";
                    case 2: return "维保商";
                    case 3: return "供应商+维保商";
                    default: return "";
                }
            }
        }
    }

    /// <summary>折旧规则（Base_DepreciationRule）。CategoryName 为展示用。</summary>
    public class BaseDepreciationRule
    {
        public int RuleID { get; set; }
        public int CategoryID { get; set; }
        public int UsefulLifeYears { get; set; }
        public decimal ResidualRate { get; set; }
        public string CategoryName { get; set; }
        public string ResidualRateText => (ResidualRate * 100m).ToString("0.##") + "%";
    }
}
