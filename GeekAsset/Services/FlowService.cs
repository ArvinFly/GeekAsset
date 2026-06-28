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
    /// 资产流转：领用/退还、借用/归还、调拨。每个操作在事务内“写流转日志 + 更新主表”，保证原子。
    /// </summary>
    public static class FlowService
    {
        private static byte StatusOf(System.Data.IDbConnection c, System.Data.IDbTransaction tx, int assetId)
            => c.ExecuteScalar<byte>("SELECT Status FROM Asset_Info WHERE AssetID=@id", new { id = assetId }, tx);

        private static int? CurrentEmp(System.Data.IDbConnection c, System.Data.IDbTransaction tx, int assetId)
            => c.ExecuteScalar<int?>("SELECT CurrentEmpID FROM Asset_Info WHERE AssetID=@id", new { id = assetId }, tx);

        private static int? CurrentLoc(System.Data.IDbConnection c, System.Data.IDbTransaction tx, int assetId)
            => c.ExecuteScalar<int?>("SELECT LocationID FROM Asset_Info WHERE AssetID=@id", new { id = assetId }, tx);

        /// <summary>领用：闲置 → 在用，绑定领用人。</summary>
        public static void Lend(int assetId, int empId)
        {
            Db.InTransaction((c, tx) =>
            {
                if (StatusOf(c, tx, assetId) != 1) throw new Exception("仅“闲置”状态的资产可领用。");
                c.Execute(@"INSERT INTO Asset_Flow_Log(AssetID,ActionType,ToEmpID,OperatorUserID,ActionTime)
                            VALUES(@a,N'领用',@e,@u,GETDATE())",
                    new { a = assetId, e = empId, u = CurrentUser.UserID }, tx);
                c.Execute("UPDATE Asset_Info SET Status=2, CurrentEmpID=@e, UpdateTime=GETDATE() WHERE AssetID=@a",
                    new { a = assetId, e = empId }, tx);
            });
        }

        /// <summary>退还：在用 → 闲置，清除领用人。</summary>
        public static void Return(int assetId)
        {
            Db.InTransaction((c, tx) =>
            {
                if (StatusOf(c, tx, assetId) != 2) throw new Exception("仅“在用”状态的资产可退还。");
                int? from = CurrentEmp(c, tx, assetId);
                c.Execute(@"INSERT INTO Asset_Flow_Log(AssetID,ActionType,FromEmpID,OperatorUserID,ActionTime)
                            VALUES(@a,N'退还',@f,@u,GETDATE())",
                    new { a = assetId, f = from, u = CurrentUser.UserID }, tx);
                c.Execute("UPDATE Asset_Info SET Status=1, CurrentEmpID=NULL, UpdateTime=GETDATE() WHERE AssetID=@a",
                    new { a = assetId }, tx);
            });
        }

        /// <summary>借用：闲置 → 借出，记录预计归还时间。</summary>
        public static void Borrow(int assetId, int empId, DateTime expectReturn)
        {
            Db.InTransaction((c, tx) =>
            {
                if (StatusOf(c, tx, assetId) != 1) throw new Exception("仅“闲置”状态的资产可借用。");
                c.Execute(@"INSERT INTO Asset_Flow_Log(AssetID,ActionType,ToEmpID,ExpectReturnDate,OperatorUserID,ActionTime)
                            VALUES(@a,N'借用',@e,@d,@u,GETDATE())",
                    new { a = assetId, e = empId, d = expectReturn, u = CurrentUser.UserID }, tx);
                c.Execute("UPDATE Asset_Info SET Status=4, CurrentEmpID=@e, UpdateTime=GETDATE() WHERE AssetID=@a",
                    new { a = assetId, e = empId }, tx);
            });
        }

        /// <summary>归还：借出 → 闲置，回填借用记录的实际归还时间。</summary>
        public static void GiveBack(int assetId)
        {
            Db.InTransaction((c, tx) =>
            {
                if (StatusOf(c, tx, assetId) != 4) throw new Exception("仅“借出”状态的资产可归还。");
                int? from = CurrentEmp(c, tx, assetId);
                // 回填最近一条未归还的借用记录
                c.Execute(@"UPDATE Asset_Flow_Log SET ActualReturnDate=GETDATE()
                            WHERE LogID=(SELECT TOP 1 LogID FROM Asset_Flow_Log
                                        WHERE AssetID=@a AND ActionType=N'借用' AND ActualReturnDate IS NULL
                                        ORDER BY LogID DESC)", new { a = assetId }, tx);
                c.Execute(@"INSERT INTO Asset_Flow_Log(AssetID,ActionType,FromEmpID,ActualReturnDate,OperatorUserID,ActionTime)
                            VALUES(@a,N'归还',@f,GETDATE(),@u,GETDATE())",
                    new { a = assetId, f = from, u = CurrentUser.UserID }, tx);
                c.Execute("UPDATE Asset_Info SET Status=1, CurrentEmpID=NULL, UpdateTime=GETDATE() WHERE AssetID=@a",
                    new { a = assetId }, tx);
            });
        }

        /// <summary>调拨：变更存放位置，记录来源/目标位置。</summary>
        public static void Transfer(int assetId, int toLocationId, string remark)
        {
            Db.InTransaction((c, tx) =>
            {
                int? from = CurrentLoc(c, tx, assetId);
                c.Execute(@"INSERT INTO Asset_Flow_Log(AssetID,ActionType,FromLocationID,ToLocationID,Remark,OperatorUserID,ActionTime)
                            VALUES(@a,N'调拨',@f,@t,@r,@u,GETDATE())",
                    new { a = assetId, f = from, t = toLocationId, r = remark, u = CurrentUser.UserID }, tx);
                c.Execute("UPDATE Asset_Info SET LocationID=@t, UpdateTime=GETDATE() WHERE AssetID=@a",
                    new { a = assetId, t = toLocationId }, tx);
            });
        }

        /// <summary>报废：任意可用状态 → 报废(5)。事务内写流转日志 + 置报废日期/原因、清领用人。报废为终态。</summary>
        public static void Scrap(int assetId, string reason)
        {
            Db.InTransaction((c, tx) =>
            {
                if (StatusOf(c, tx, assetId) == 5) throw new Exception("该资产已是报废状态。");
                int? from = CurrentEmp(c, tx, assetId);
                c.Execute(@"INSERT INTO Asset_Flow_Log(AssetID,ActionType,FromEmpID,Remark,OperatorUserID,ActionTime)
                            VALUES(@a,N'报废',@f,@r,@u,GETDATE())",
                    new { a = assetId, f = from, r = reason, u = CurrentUser.UserID }, tx);
                c.Execute(@"UPDATE Asset_Info SET Status=5, ScrapDate=GETDATE(), ScrapReason=@r,
                            CurrentEmpID=NULL, UpdateTime=GETDATE() WHERE AssetID=@a",
                    new { a = assetId, r = reason }, tx);
            });
        }

        /// <summary>超时未还的借用资产。</summary>
        public static List<OverdueBorrow> GetOverdue()
        {
            using (var c = Db.Open())
                return c.Query<OverdueBorrow>(
                    @"SELECT a.AssetID, a.AssetNo, a.AssetName, e.EmpName, e.Mobile, f.ExpectReturnDate
                      FROM Asset_Flow_Log f
                      JOIN Asset_Info a ON f.AssetID=a.AssetID
                      LEFT JOIN Sys_Employee e ON f.ToEmpID=e.EmpID
                      WHERE f.ActionType=N'借用' AND f.ActualReturnDate IS NULL
                        AND f.ExpectReturnDate IS NOT NULL AND f.ExpectReturnDate < CAST(GETDATE() AS DATE)
                      ORDER BY f.ExpectReturnDate").ToList();
        }

        /// <summary>单台资产履历（流转 + 硬件日志，按时间倒序）。</summary>
        public static List<HistoryItem> GetHistory(int assetId)
        {
            using (var c = Db.Open())
            {
                var flow = c.Query<FlowHistoryRow>(
                    @"SELECT f.ActionTime AS Time, f.ActionType,
                             fe.EmpName AS FromEmp, te.EmpName AS ToEmp,
                             fl.LocationName AS FromLoc, tl.LocationName AS ToLoc,
                             f.ExpectReturnDate, f.ActualReturnDate, f.Remark, u.RealName AS OperatorName
                      FROM Asset_Flow_Log f
                      LEFT JOIN Sys_Employee fe ON f.FromEmpID=fe.EmpID
                      LEFT JOIN Sys_Employee te ON f.ToEmpID=te.EmpID
                      LEFT JOIN Base_Location fl ON f.FromLocationID=fl.LocationID
                      LEFT JOIN Base_Location tl ON f.ToLocationID=tl.LocationID
                      LEFT JOIN Sys_User u ON f.OperatorUserID=u.UserID
                      WHERE f.AssetID=@id", new { id = assetId });

                var hw = c.Query<HwHistoryRow>(
                    @"SELECT h.ChangeTime AS Time, h.ChangeType, h.PartName, h.Old_Spec AS OldSpec, h.New_Spec AS NewSpec,
                             h.Cost, u.RealName AS HandlerName, h.Remark
                      FROM Asset_Hardware_Log h
                      LEFT JOIN Sys_User u ON h.HandlerUserID=u.UserID
                      WHERE h.AssetID=@id", new { id = assetId });

                var items = new List<HistoryItem>();
                foreach (var f in flow)
                    items.Add(new HistoryItem { Time = f.Time, Kind = "流转", Action = f.ActionType, Detail = FlowDetail(f), Operator = f.OperatorName });
                foreach (var h in hw)
                    items.Add(new HistoryItem { Time = h.Time, Kind = "硬件", Action = h.ChangeType, Detail = HwDetail(h), Operator = h.HandlerName });

                return items.OrderByDescending(x => x.Time).ToList();
            }
        }

        private static string FlowDetail(FlowHistoryRow f)
        {
            switch (f.ActionType)
            {
                case "领用": return "领用人：" + f.ToEmp;
                case "退还": return "退还人：" + f.FromEmp;
                case "借用": return $"借用人：{f.ToEmp}，预计归还：{f.ExpectReturnDate:yyyy-MM-dd}";
                case "归还": return "归还人：" + f.FromEmp;
                case "调拨": return $"{f.FromLoc ?? "(无)"} → {f.ToLoc}" + (string.IsNullOrEmpty(f.Remark) ? "" : "，" + f.Remark);
                case "报废": return string.IsNullOrEmpty(f.Remark) ? "报废处置" : "报废原因：" + f.Remark;
                default: return f.Remark;
            }
        }

        private static string HwDetail(HwHistoryRow h)
        {
            string s = h.PartName;
            if (!string.IsNullOrEmpty(h.OldSpec) || !string.IsNullOrEmpty(h.NewSpec))
                s += $" [{h.OldSpec} → {h.NewSpec}]";
            if (h.Cost > 0) s += $"，费用 {h.Cost:0.##}";
            return s;
        }
    }
}
