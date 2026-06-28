using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;
using MiniExcelLibs;

namespace GeekAsset.Services
{
    public static class AssetService
    {
        private const string SelectList =
            @"SELECT a.AssetID, a.AssetNo, a.AssetName, cat.CategoryName, a.Model, a.Status,
                     loc.LocationName, emp.EmpName, sup.SupplierName,
                     a.PurchasePrice, a.PurchaseDate, a.WarrantyExpireDate, a.SN
              FROM Asset_Info a
              LEFT JOIN Base_AssetCategory cat ON a.CategoryID=cat.CategoryID
              LEFT JOIN Base_Location loc      ON a.LocationID=loc.LocationID
              LEFT JOIN Sys_Employee emp       ON a.CurrentEmpID=emp.EmpID
              LEFT JOIN Base_Supplier sup      ON a.SupplierID=sup.SupplierID
              WHERE a.IsDeleted=0 ORDER BY a.AssetID DESC";

        public static List<AssetListItem> GetAll()
        {
            using (var c = Db.Open()) return c.Query<AssetListItem>(SelectList).ToList();
        }

        public static AssetInfo GetById(int id)
        {
            using (var c = Db.Open())
                return c.QueryFirstOrDefault<AssetInfo>("SELECT * FROM Asset_Info WHERE AssetID=@id", new { id });
        }

        /// <summary>生成下一个资产编号：ZC + yyyyMM + 4 位流水。</summary>
        public static string NextAssetNo()
        {
            string prefix = "ZC" + DateTime.Now.ToString("yyyyMM");
            using (var c = Db.Open())
            {
                int max = c.ExecuteScalar<int?>(
                    "SELECT MAX(CAST(RIGHT(AssetNo,4) AS INT)) FROM Asset_Info WHERE AssetNo LIKE @p",
                    new { p = prefix + "%" }) ?? 0;
                return prefix + (max + 1).ToString("D4");
            }
        }

        public static bool AssetNoExists(string assetNo, int excludeId)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM Asset_Info WHERE AssetNo=@no AND AssetID<>@id", new { no = assetNo, id = excludeId }) > 0;
        }

        public static int Insert(AssetInfo a)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Asset_Info
                        (AssetNo, AssetName, CategoryID, Model, Status, LocationID, CurrentEmpID,
                         SupplierID, PurchaseDate, PurchasePrice, WarrantyExpireDate,
                         SN, MacAddress, IPAddress, IPType, Original_Spec, Current_Spec, Remark, CreateBy, CreateTime)
                      VALUES
                        (@AssetNo,@AssetName,@CategoryID,@Model,@Status,@LocationID,@CurrentEmpID,
                         @SupplierID,@PurchaseDate,@PurchasePrice,@WarrantyExpireDate,
                         @SN,@MacAddress,@IPAddress,@IPType,@Original_Spec,@Current_Spec,@Remark,@CreateBy,GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() AS INT);", a);
        }

        public static void Update(AssetInfo a)
        {
            using (var c = Db.Open())
                c.Execute(
                    @"UPDATE Asset_Info SET
                        AssetNo=@AssetNo, AssetName=@AssetName, CategoryID=@CategoryID, Model=@Model, Status=@Status,
                        LocationID=@LocationID, CurrentEmpID=@CurrentEmpID, SupplierID=@SupplierID,
                        PurchaseDate=@PurchaseDate, PurchasePrice=@PurchasePrice, WarrantyExpireDate=@WarrantyExpireDate,
                        SN=@SN, MacAddress=@MacAddress, IPAddress=@IPAddress, IPType=@IPType,
                        Current_Spec=@Current_Spec, Remark=@Remark, UpdateTime=GETDATE()
                      WHERE AssetID=@AssetID", a);
        }

        // 报废已迁至 FlowService.Scrap（事务内写流转日志 + 主表，统一履历时间线）。

        public static void SoftDelete(int assetId)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Asset_Info SET IsDeleted=1 WHERE AssetID=@id", new { id = assetId });
        }

        // ============ Excel 导入导出（MiniExcel） ============
        private static readonly string[] Headers =
        {
            "资产编号","资产名称","资产分类","规格型号","状态","存放位置","当前领用人","供应商",
            "采购日期","含税原值","维保到期日","SN序列号","MAC地址","IP地址","IP类型","原始配置","当前配置","备注"
        };

        /// <summary>导出当前台账到 Excel。</summary>
        public static void ExportExcel(string path)
        {
            var list = GetAll();
            var rows = list.Select(a => new Dictionary<string, object>
            {
                ["资产编号"] = a.AssetNo,
                ["资产名称"] = a.AssetName,
                ["资产分类"] = a.CategoryName,
                ["规格型号"] = a.Model,
                ["状态"] = a.StatusText,
                ["存放位置"] = a.LocationName,
                ["当前领用人"] = a.EmpName,
                ["供应商"] = a.SupplierName,
                ["采购日期"] = a.PurchaseDate?.ToString("yyyy-MM-dd"),
                ["含税原值"] = a.PurchasePrice,
                ["维保到期日"] = a.WarrantyExpireDate?.ToString("yyyy-MM-dd"),
                ["SN序列号"] = a.SN,
            }).ToList();
            if (rows.Count == 0) rows.Add(Headers.ToDictionary(h => h, h => (object)null));
            MiniExcel.SaveAs(path, rows, overwriteFile: true);
        }

        /// <summary>生成空白导入模板（仅表头 + 一行示例）。</summary>
        public static void ExportTemplate(string path)
        {
            var sample = new Dictionary<string, object>
            {
                ["资产编号"] = "(留空则自动生成)",
                ["资产名称"] = "示例：研发笔记本",
                ["资产分类"] = "笔记本",
                ["规格型号"] = "ThinkPad X1",
                ["状态"] = "闲置",
                ["存放位置"] = "IT备件库房",
                ["当前领用人"] = "",
                ["供应商"] = "",
                ["采购日期"] = "2026-01-01",
                ["含税原值"] = 8999,
                ["维保到期日"] = "2029-01-01",
                ["SN序列号"] = "SN123456",
                ["MAC地址"] = "",
                ["IP地址"] = "",
                ["IP类型"] = "DHCP",
                ["原始配置"] = "i7/16G/512G",
                ["当前配置"] = "i7/16G/512G",
                ["备注"] = "",
            };
            MiniExcel.SaveAs(path, new List<Dictionary<string, object>> { sample }, overwriteFile: true);
        }

        /// <summary>从 Excel 导入资产。返回 (成功数, 失败行描述列表)。按名称匹配分类/位置/人员/供应商。</summary>
        public static (int ok, List<string> errors) ImportExcel(string path)
        {
            var errors = new List<string>();
            int ok = 0;

            // 预加载名称→ID 字典
            var catMap = CategoryService.GetAll().GroupBy(x => x.CategoryName).ToDictionary(g => g.Key, g => g.First().CategoryID);
            var locMap = LocationService.GetAll().GroupBy(x => x.LocationName).ToDictionary(g => g.Key, g => g.First().LocationID);
            var empMap = EmployeeService.GetAll().GroupBy(x => x.EmpName).ToDictionary(g => g.Key, g => g.First().EmpID);
            var supMap = SupplierService.GetAll().GroupBy(x => x.SupplierName).ToDictionary(g => g.Key, g => g.First().SupplierID);

            var rows = MiniExcel.Query(path, useHeaderRow: true).Cast<IDictionary<string, object>>().ToList();
            int line = 1;
            foreach (var r in rows)
            {
                line++;
                try
                {
                    string name = Str(r, "资产名称");
                    if (string.IsNullOrWhiteSpace(name)) { continue; } // 跳过空行

                    var a = new AssetInfo
                    {
                        AssetName = name,
                        Model = Str(r, "规格型号"),
                        SN = Str(r, "SN序列号"),
                        MacAddress = Str(r, "MAC地址"),
                        IPAddress = Str(r, "IP地址"),
                        Original_Spec = Str(r, "原始配置"),
                        Current_Spec = Str(r, "当前配置"),
                        Remark = Str(r, "备注"),
                        Status = StatusFromText(Str(r, "状态")),
                        IPType = IpTypeFromText(Str(r, "IP类型")),
                        PurchaseDate = Date(r, "采购日期"),
                        WarrantyExpireDate = Date(r, "维保到期日"),
                        PurchasePrice = Dec(r, "含税原值"),
                    };

                    string catName = Str(r, "资产分类");
                    if (!string.IsNullOrWhiteSpace(catName))
                    {
                        if (!catMap.TryGetValue(catName, out int cid)) { errors.Add($"第{line}行：分类「{catName}」不存在"); continue; }
                        a.CategoryID = cid;
                    }
                    string locName = Str(r, "存放位置");
                    if (!string.IsNullOrWhiteSpace(locName) && locMap.TryGetValue(locName, out int lid)) a.LocationID = lid;
                    string empName = Str(r, "当前领用人");
                    if (!string.IsNullOrWhiteSpace(empName) && empMap.TryGetValue(empName, out int eid)) a.CurrentEmpID = eid;
                    string supName = Str(r, "供应商");
                    if (!string.IsNullOrWhiteSpace(supName) && supMap.TryGetValue(supName, out int sid)) a.SupplierID = sid;

                    string no = Str(r, "资产编号");
                    a.AssetNo = string.IsNullOrWhiteSpace(no) || no.StartsWith("(") ? NextAssetNo() : no;
                    if (AssetNoExists(a.AssetNo, 0)) { errors.Add($"第{line}行：资产编号「{a.AssetNo}」已存在"); continue; }

                    Insert(a);
                    ok++;
                }
                catch (Exception ex)
                {
                    errors.Add($"第{line}行：{ex.Message}");
                }
            }
            return (ok, errors);
        }

        // ---- 单元格解析辅助 ----
        private static string Str(IDictionary<string, object> r, string key)
            => r.TryGetValue(key, out var v) && v != null ? v.ToString().Trim() : null;

        private static decimal Dec(IDictionary<string, object> r, string key)
            => decimal.TryParse(Str(r, key), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;

        private static DateTime? Date(IDictionary<string, object> r, string key)
        {
            string s = Str(r, key);
            if (string.IsNullOrWhiteSpace(s)) return null;
            return DateTime.TryParse(s, out var dt) ? (DateTime?)dt : null;
        }

        private static byte StatusFromText(string s)
        {
            switch (s)
            {
                case "在用": return 2;
                case "报修": return 3;
                case "借出": return 4;
                case "报废": return 5;
                default: return 1; // 闲置
            }
        }

        private static byte? IpTypeFromText(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (s.Contains("固定")) return 1;
            if (s.IndexOf("DHCP", StringComparison.OrdinalIgnoreCase) >= 0) return 2;
            return null;
        }
    }
}
