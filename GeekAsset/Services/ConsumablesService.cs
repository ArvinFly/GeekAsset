using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;
using GeekAsset.Security;

namespace GeekAsset.Services
{
    /// <summary>低值易耗品：台账维护 + 出入库（事务内改库存并写流水；领用带 WHERE 防超扣）。</summary>
    public static class ConsumablesService
    {
        public static List<ConsumableInfo> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<ConsumableInfo>(
                    @"SELECT ConsumableID, ConsumableName, Spec, Unit, CategoryName, Inventory, SafetyStock
                      FROM Consumables_Info WHERE IsDeleted=0 ORDER BY ConsumableName").ToList();
        }

        public static int Insert(string name, string spec, string unit, string category, int safetyStock)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Consumables_Info(ConsumableName,Spec,Unit,CategoryName,Inventory,SafetyStock)
                      VALUES(@n,@sp,@u,@cat,0,@ss);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { n = name, sp = spec, u = unit, cat = category, ss = safetyStock });
        }

        public static void Update(int id, string name, string spec, string unit, string category, int safetyStock)
        {
            using (var c = Db.Open())
                c.Execute(@"UPDATE Consumables_Info SET ConsumableName=@n, Spec=@sp, Unit=@u, CategoryName=@cat, SafetyStock=@ss
                            WHERE ConsumableID=@id",
                    new { n = name, sp = spec, u = unit, cat = category, ss = safetyStock, id });
        }

        public static void SoftDelete(int id)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Consumables_Info SET IsDeleted=1 WHERE ConsumableID=@id", new { id });
        }

        /// <summary>入库：加库存并写入库流水。</summary>
        public static void StockIn(int id, int qty, string remark)
        {
            if (qty <= 0) throw new Exception("入库数量须为正数。");
            Db.InTransaction((c, tx) =>
            {
                c.Execute("UPDATE Consumables_Info SET Inventory=Inventory+@q WHERE ConsumableID=@id", new { q = qty, id }, tx);
                int bal = c.ExecuteScalar<int>("SELECT Inventory FROM Consumables_Info WHERE ConsumableID=@id", new { id }, tx);
                c.Execute(@"INSERT INTO Consumables_Log(ConsumableID,ActionType,Quantity,BalanceAfter,OperatorUserID,ActionTime,Remark)
                            VALUES(@id,1,@q,@bal,@u,GETDATE(),@r)",
                    new { id, q = qty, bal, u = CurrentUser.UserID, r = remark }, tx);
            });
        }

        /// <summary>领用：带 WHERE Inventory>=@q 防超扣；不足则抛异常。</summary>
        public static void Issue(int id, int qty, int? receiverEmpId, string remark)
        {
            if (qty <= 0) throw new Exception("领用数量须为正数。");
            Db.InTransaction((c, tx) =>
            {
                int affected = c.Execute(
                    "UPDATE Consumables_Info SET Inventory=Inventory-@q WHERE ConsumableID=@id AND Inventory>=@q",
                    new { q = qty, id }, tx);
                if (affected == 0) throw new Exception("库存不足，领用失败。");
                int bal = c.ExecuteScalar<int>("SELECT Inventory FROM Consumables_Info WHERE ConsumableID=@id", new { id }, tx);
                c.Execute(@"INSERT INTO Consumables_Log(ConsumableID,ActionType,Quantity,BalanceAfter,ReceiverEmpID,OperatorUserID,ActionTime,Remark)
                            VALUES(@id,2,@q,@bal,@rcv,@u,GETDATE(),@r)",
                    new { id, q = qty, bal, rcv = receiverEmpId, u = CurrentUser.UserID, r = remark }, tx);
            });
        }

        public static List<ConsumableLogRow> GetLogs(int id)
        {
            using (var c = Db.Open())
                return c.Query<ConsumableLogRow>(
                    @"SELECT g.LogID, g.ActionType, g.Quantity, g.BalanceAfter,
                             e.EmpName AS ReceiverName, u.RealName AS OperatorName, g.ActionTime, g.Remark
                      FROM Consumables_Log g
                      LEFT JOIN Sys_Employee e ON g.ReceiverEmpID=e.EmpID
                      LEFT JOIN Sys_User u ON g.OperatorUserID=u.UserID
                      WHERE g.ConsumableID=@id
                      ORDER BY g.LogID DESC", new { id }).ToList();
        }
    }
}
