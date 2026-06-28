using System.Collections.Generic;
using System.Linq;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;

namespace GeekAsset.Services
{
    /// <summary>报表看板：资产分布、价值、预警等聚合统计（均排除已作废 IsDeleted）。</summary>
    public static class ReportService
    {
        public static DashboardStats GetStats()
        {
            using (var c = Db.Open())
            {
                var s = c.QueryFirst<DashboardStats>(
                    @"SELECT
                        COUNT(*) AS TotalAssets,
                        ISNULL(SUM(PurchasePrice),0) AS TotalValue,
                        ISNULL(SUM(CASE WHEN Status=1 THEN 1 ELSE 0 END),0) AS Idle,
                        ISNULL(SUM(CASE WHEN Status=2 THEN 1 ELSE 0 END),0) AS InUse,
                        ISNULL(SUM(CASE WHEN Status=3 THEN 1 ELSE 0 END),0) AS Repair,
                        ISNULL(SUM(CASE WHEN Status=4 THEN 1 ELSE 0 END),0) AS Borrowed,
                        ISNULL(SUM(CASE WHEN Status=5 THEN 1 ELSE 0 END),0) AS Scrapped
                      FROM Asset_Info WHERE IsDeleted=0");

                s.WarrantySoon = c.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM Asset_Info
                      WHERE IsDeleted=0 AND Status<>5 AND WarrantyExpireDate IS NOT NULL
                        AND WarrantyExpireDate >= CAST(GETDATE() AS DATE)
                        AND WarrantyExpireDate < DATEADD(DAY,30,CAST(GETDATE() AS DATE))");

                s.OverdueBorrow = c.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM Asset_Flow_Log
                      WHERE ActionType=N'借用' AND ActualReturnDate IS NULL
                        AND ExpectReturnDate IS NOT NULL AND ExpectReturnDate < CAST(GETDATE() AS DATE)");

                s.LowStock = c.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Consumables_Info WHERE IsDeleted=0 AND SafetyStock>0 AND Inventory<=SafetyStock");

                return s;
            }
        }

        /// <summary>30天内维保到期清单（工作台预警）。</summary>
        public static List<WarrantyWarnRow> WarrantySoonList()
        {
            using (var c = Db.Open())
                return c.Query<WarrantyWarnRow>(
                    @"SELECT AssetNo, AssetName, WarrantyExpireDate
                      FROM Asset_Info
                      WHERE IsDeleted=0 AND Status<>5 AND WarrantyExpireDate IS NOT NULL
                        AND WarrantyExpireDate >= CAST(GETDATE() AS DATE)
                        AND WarrantyExpireDate < DATEADD(DAY,30,CAST(GETDATE() AS DATE))
                      ORDER BY WarrantyExpireDate").ToList();
        }

        /// <summary>按状态分布（数量 + 原值）。</summary>
        public static List<StatRow> ByStatus()
        {
            using (var c = Db.Open())
            {
                var rows = c.Query(
                    @"SELECT Status, COUNT(*) AS Cnt, ISNULL(SUM(PurchasePrice),0) AS Val
                      FROM Asset_Info WHERE IsDeleted=0 GROUP BY Status ORDER BY Status").ToList();
                return rows.Select(r => new StatRow
                {
                    Name = AssetStatus.Text((byte)r.Status),
                    Count = (int)r.Cnt,
                    Value = (decimal)r.Val
                }).ToList();
            }
        }

        /// <summary>按分类分布（数量 + 原值）。</summary>
        public static List<StatRow> ByCategory()
        {
            using (var c = Db.Open())
                return c.Query<StatRow>(
                    @"SELECT ISNULL(cat.CategoryName,N'(未分类)') AS Name, COUNT(*) AS Count, ISNULL(SUM(a.PurchasePrice),0) AS Value
                      FROM Asset_Info a
                      LEFT JOIN Base_AssetCategory cat ON a.CategoryID=cat.CategoryID
                      WHERE a.IsDeleted=0
                      GROUP BY cat.CategoryName
                      ORDER BY COUNT(*) DESC").ToList();
        }
    }
}
