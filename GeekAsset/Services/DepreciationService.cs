using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;
using GeekAsset.Security;

namespace GeekAsset.Services
{
    /// <summary>
    /// 财务折旧（直线法）批量计提，恪守折旧三铁律：
    ///   A 当月增加次月计提：PurchaseDate 必须早于计提月首日才参与本月计提；
    ///   B 残值封顶：净值不跌破残值，最后一月只提剩余可折额并置 IsDepreciated=1；
    ///   C 幂等防重：(AssetID,DeprecYear,DeprecMonth) 唯一 + 计提前 IF NOT EXISTS 跳过。
    /// 月折旧额 = (原值 - 残值) / (预计年限 × 12)。残值取 Asset_Info.ResidualValue，缺失则 原值×分类残值率。
    /// </summary>
    public static class DepreciationService
    {
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        public static DepreciationResult Run(int year, int month)
        {
            var result = new DepreciationResult { Year = year, Month = month };
            var firstDay = new DateTime(year, month, 1);

            Db.InTransaction((c, tx) =>
            {
                // 符合条件的在册资产 + 其分类折旧规则（铁律A：购置日早于计提月首日）
                var assets = c.Query(
                    @"SELECT a.AssetID, a.PurchasePrice, a.ResidualValue, r.UsefulLifeYears, r.ResidualRate
                      FROM Asset_Info a
                      JOIN Base_DepreciationRule r ON a.CategoryID = r.CategoryID
                      WHERE a.IsDeleted=0 AND a.Status<>5 AND a.IsDepreciated=0
                        AND a.PurchasePrice > 0 AND a.PurchaseDate IS NOT NULL
                        AND a.PurchaseDate < @firstDay",
                    new { firstDay }, tx).ToList();

                foreach (var a in assets)
                {
                    int assetId = (int)a.AssetID;

                    // 铁律C：本月已提则跳过
                    bool exists = c.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Asset_Depreciation_Log WHERE AssetID=@a AND DeprecYear=@y AND DeprecMonth=@m",
                        new { a = assetId, y = year, m = month }, tx) > 0;
                    if (exists) { result.Skipped++; continue; }

                    decimal price = (decimal)a.PurchasePrice;
                    int life = (int)a.UsefulLifeYears;
                    decimal rate = (decimal)a.ResidualRate;
                    decimal residual = a.ResidualValue != null ? (decimal)a.ResidualValue : R2(price * rate);
                    if (residual < 0) residual = 0;
                    if (residual > price) residual = price;

                    // 上一期累计/净值（无则从原值起算）
                    var prev = c.QueryFirstOrDefault(
                        @"SELECT TOP 1 AccumulatedAmount, NetValue FROM Asset_Depreciation_Log
                          WHERE AssetID=@a ORDER BY DeprecYear DESC, DeprecMonth DESC",
                        new { a = assetId }, tx);
                    decimal accumulated = prev != null ? (decimal)prev.AccumulatedAmount : 0m;
                    decimal netValue = prev != null ? (decimal)prev.NetValue : price;

                    decimal remaining = netValue - residual;       // 还可折额
                    if (remaining <= 0)
                    {
                        // 已到残值，标记折完，本期不提
                        c.Execute("UPDATE Asset_Info SET IsDepreciated=1, UpdateTime=GETDATE() WHERE AssetID=@a",
                            new { a = assetId }, tx);
                        continue;
                    }

                    decimal monthly = life > 0 ? R2((price - residual) / (life * 12)) : remaining;
                    if (monthly <= 0) continue;

                    decimal thisAmount = Math.Min(monthly, remaining);   // 铁律B：残值封顶
                    decimal newAccum = R2(accumulated + thisAmount);
                    decimal newNet = R2(netValue - thisAmount);
                    if (newNet < residual) newNet = residual;

                    c.Execute(
                        @"INSERT INTO Asset_Depreciation_Log
                            (AssetID,DeprecYear,DeprecMonth,MonthlyAmount,AccumulatedAmount,NetValue,GenerateUserID,GenerateTime)
                          VALUES(@a,@y,@m,@amt,@accum,@net,@u,GETDATE())",
                        new { a = assetId, y = year, m = month, amt = thisAmount, accum = newAccum, net = newNet, u = CurrentUser.UserID }, tx);

                    result.Processed++;
                    result.TotalAmount += thisAmount;

                    if (newNet <= residual)   // 折完封顶
                    {
                        c.Execute("UPDATE Asset_Info SET IsDepreciated=1, UpdateTime=GETDATE() WHERE AssetID=@a",
                            new { a = assetId }, tx);
                        result.Completed++;
                    }
                }
            });

            result.TotalAmount = R2(result.TotalAmount);
            return result;
        }

        /// <summary>某期已计提折旧流水（联表取资产名称）。</summary>
        public static List<DepreciationRow> GetLogs(int year, int month)
        {
            using (var c = Db.Open())
                return c.Query<DepreciationRow>(
                    @"SELECT d.AssetID, a.AssetNo, a.AssetName, d.DeprecYear, d.DeprecMonth,
                             d.MonthlyAmount, d.AccumulatedAmount, d.NetValue, d.GenerateTime
                      FROM Asset_Depreciation_Log d
                      JOIN Asset_Info a ON d.AssetID=a.AssetID
                      WHERE d.DeprecYear=@y AND d.DeprecMonth=@m
                      ORDER BY a.AssetNo", new { y = year, m = month }).ToList();
        }
    }
}
