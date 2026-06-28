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
    /// 扫码盘点：建盘点单(按范围快照在册资产) → 扫码/勾选盘点 → 盘盈盘亏报告 → 完成结单。
    /// 明细差异口径：正常1 / 盘盈2(扫到范围外或未知条码) / 盘亏3(结单时仍未盘) / 信息不符4(实际人或位置与账面不一致)。
    /// </summary>
    public static class AuditService
    {
        /// <summary>盘点任务列表（含明细统计），按开始时间倒序。</summary>
        public static List<AuditTaskItem> GetTasks()
        {
            using (var c = Db.Open())
                return c.Query<AuditTaskItem>(
                    @"SELECT t.TaskID, t.TaskName, t.ScopeType, t.Status, t.StartTime, t.EndTime, t.Remark,
                             CASE t.ScopeType WHEN 1 THEN d.DeptName WHEN 2 THEN l.LocationName WHEN 3 THEN cat.CategoryName END AS ScopeName,
                             (SELECT COUNT(*) FROM Audit_Detail ad WHERE ad.TaskID=t.TaskID) AS Total,
                             (SELECT COUNT(*) FROM Audit_Detail ad WHERE ad.TaskID=t.TaskID AND ad.AuditStatus=2) AS Audited
                      FROM Audit_Task t
                      LEFT JOIN Sys_Dept d ON t.ScopeType=1 AND t.ScopeRefID=d.DeptID
                      LEFT JOIN Base_Location l ON t.ScopeType=2 AND t.ScopeRefID=l.LocationID
                      LEFT JOIN Base_AssetCategory cat ON t.ScopeType=3 AND t.ScopeRefID=cat.CategoryID
                      ORDER BY t.StartTime DESC").ToList();
        }

        /// <summary>新建盘点单：建任务 + 把范围内在册资产(排除已报废/作废)快照成未盘明细。返回 TaskID。</summary>
        public static int CreateTask(string name, byte scopeType, int? scopeRefId, string remark)
        {
            int taskId = 0;
            Db.InTransaction((c, tx) =>
            {
                taskId = c.ExecuteScalar<int>(
                    @"INSERT INTO Audit_Task(TaskName,ScopeType,ScopeRefID,Status,StartTime,CreateUserID,Remark)
                      VALUES(@n,@st,@ref,1,GETDATE(),@u,@r);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { n = name, st = scopeType, @ref = scopeRefId, u = CurrentUser.UserID, r = remark }, tx);

                // 范围过滤：1=部门(经持有人 DeptID) 2=位置 3=分类
                string where = "a.IsDeleted=0 AND a.Status<>5";
                if (scopeRefId.HasValue)
                {
                    if (scopeType == 1) where += " AND a.CurrentEmpID IN (SELECT EmpID FROM Sys_Employee WHERE DeptID=@ref)";
                    else if (scopeType == 2) where += " AND a.LocationID=@ref";
                    else if (scopeType == 3) where += " AND a.CategoryID=@ref";
                }

                c.Execute(
                    $@"INSERT INTO Audit_Detail(TaskID,AssetID,AuditStatus,BookEmpID,BookLocationID)
                       SELECT @tid, a.AssetID, 1, a.CurrentEmpID, a.LocationID
                       FROM Asset_Info a WHERE {where}",
                    new { tid = taskId, @ref = scopeRefId }, tx);
            });
            return taskId;
        }

        /// <summary>某盘点单的明细（联表取名称），未盘在前。</summary>
        public static List<AuditDetailItem> GetDetails(int taskId)
        {
            using (var c = Db.Open())
                return c.Query<AuditDetailItem>(
                    @"SELECT ad.DetailID, ad.AssetID, a.AssetNo, a.AssetName, ad.ScannedCode,
                             ad.AuditStatus, ad.ResultType,
                             be.EmpName AS BookEmpName, ae.EmpName AS ActualEmpName,
                             bl.LocationName AS BookLocName, al.LocationName AS ActualLocName,
                             ad.ScanTime, ad.Remark
                      FROM Audit_Detail ad
                      LEFT JOIN Asset_Info a ON ad.AssetID=a.AssetID
                      LEFT JOIN Sys_Employee be ON ad.BookEmpID=be.EmpID
                      LEFT JOIN Sys_Employee ae ON ad.ActualEmpID=ae.EmpID
                      LEFT JOIN Base_Location bl ON ad.BookLocationID=bl.LocationID
                      LEFT JOIN Base_Location al ON ad.ActualLocationID=al.LocationID
                      WHERE ad.TaskID=@tid
                      ORDER BY ad.AuditStatus, ad.DetailID", new { tid = taskId }).ToList();
        }

        /// <summary>扫码盘点：按资产编号在本单内核销。返回结果文案。</summary>
        public static string Scan(int taskId, string code)
        {
            code = (code ?? "").Trim();
            if (code.Length == 0) return "条码为空";
            using (var c = Db.Open())
            {
                var asset = c.QueryFirstOrDefault(
                    "SELECT AssetID, AssetName FROM Asset_Info WHERE AssetNo=@no AND IsDeleted=0",
                    new { no = code });

                if (asset == null)
                {
                    // 未知条码 → 盘盈（无账有物）
                    c.Execute(@"INSERT INTO Audit_Detail(TaskID,AssetID,ScannedCode,AuditStatus,ResultType,ScanTime)
                                VALUES(@t,NULL,@code,2,2,GETDATE())", new { t = taskId, code });
                    return $"未知条码 {code} → 记为盘盈";
                }

                int aid = (int)asset.AssetID;
                string aname = (string)asset.AssetName;
                var detailId = c.ExecuteScalar<long?>(
                    "SELECT TOP 1 DetailID FROM Audit_Detail WHERE TaskID=@t AND AssetID=@a ORDER BY DetailID",
                    new { t = taskId, a = aid });

                if (detailId == null)
                {
                    // 范围外资产被扫到 → 盘盈
                    c.Execute(@"INSERT INTO Audit_Detail(TaskID,AssetID,ScannedCode,AuditStatus,ResultType,ScanTime)
                                VALUES(@t,@a,@code,2,2,GETDATE())", new { t = taskId, a = aid, code });
                    return $"{aname}（范围外）→ 记为盘盈";
                }

                // 在册资产扫到 → 正常
                c.Execute(@"UPDATE Audit_Detail SET AuditStatus=2, ResultType=1, ScannedCode=@code, ScanTime=GETDATE()
                            WHERE DetailID=@d AND AuditStatus=1", new { d = detailId.Value, code });
                return $"{aname} → 已盘（正常）";
            }
        }

        /// <summary>勾选盘点（手工核销，可校正实际使用人/位置）：与账面比对得正常/信息不符。</summary>
        public static void CheckOff(long detailId, int? actualEmpId, int? actualLocId, string remark)
        {
            using (var c = Db.Open())
            {
                var book = c.QueryFirst(
                    "SELECT BookEmpID, BookLocationID FROM Audit_Detail WHERE DetailID=@d", new { d = detailId });
                int? bookEmp = (int?)book.BookEmpID;
                int? bookLoc = (int?)book.BookLocationID;
                bool mismatch = (actualEmpId.HasValue && actualEmpId != bookEmp)
                             || (actualLocId.HasValue && actualLocId != bookLoc);
                c.Execute(@"UPDATE Audit_Detail
                            SET AuditStatus=2, ResultType=@rt, ActualEmpID=@ae, ActualLocationID=@al,
                                Remark=@rm, ScanTime=GETDATE()
                            WHERE DetailID=@d",
                    new { d = detailId, rt = (byte)(mismatch ? 4 : 1), ae = actualEmpId, al = actualLocId, rm = remark });
            }
        }

        /// <summary>撤销盘点：明细回到未盘。</summary>
        public static void Reset(long detailId)
        {
            using (var c = Db.Open())
                c.Execute(@"UPDATE Audit_Detail SET AuditStatus=1, ResultType=NULL, ScannedCode=NULL,
                            ActualEmpID=NULL, ActualLocationID=NULL, ScanTime=NULL WHERE DetailID=@d",
                    new { d = detailId });
        }

        /// <summary>盘盈盘亏汇总。结单后未盘均计盘亏；结单前未盘计入 Pending。</summary>
        public static AuditReport GetReport(int taskId)
        {
            using (var c = Db.Open())
                return c.QueryFirst<AuditReport>(
                    @"SELECT
                        COUNT(*) AS Total,
                        ISNULL(SUM(CASE WHEN AuditStatus=2 THEN 1 ELSE 0 END),0) AS Audited,
                        ISNULL(SUM(CASE WHEN ResultType=1 THEN 1 ELSE 0 END),0) AS Normal,
                        ISNULL(SUM(CASE WHEN ResultType=2 THEN 1 ELSE 0 END),0) AS Surplus,
                        ISNULL(SUM(CASE WHEN ResultType=3 THEN 1 ELSE 0 END),0) AS Deficit,
                        ISNULL(SUM(CASE WHEN ResultType=4 THEN 1 ELSE 0 END),0) AS Mismatch
                      FROM Audit_Detail WHERE TaskID=@tid", new { tid = taskId });
        }

        /// <summary>结单：未盘明细计为盘亏，任务置已完成。</summary>
        public static void FinishTask(int taskId)
        {
            Db.InTransaction((c, tx) =>
            {
                c.Execute("UPDATE Audit_Detail SET ResultType=3 WHERE TaskID=@t AND AuditStatus=1", new { t = taskId }, tx);
                c.Execute("UPDATE Audit_Task SET Status=2, EndTime=GETDATE() WHERE TaskID=@t", new { t = taskId }, tx);
            });
        }

        /// <summary>删除盘点单（连同明细）。</summary>
        public static void DeleteTask(int taskId)
        {
            Db.InTransaction((c, tx) =>
            {
                c.Execute("DELETE FROM Audit_Detail WHERE TaskID=@t", new { t = taskId }, tx);
                c.Execute("DELETE FROM Audit_Task WHERE TaskID=@t", new { t = taskId }, tx);
            });
        }
    }
}
