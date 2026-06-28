using GeekAsset.Data;
using GeekAsset.Models;
using GeekAsset.Security;
using Dapper;

namespace GeekAsset.Services
{
    /// <summary>
    /// 维修维保登记：写 Asset_Hardware_Log（换件/升级/维修），并按变更后配置同步主表 Current_Spec。
    /// Old_Spec/New_Spec 为防调包配置快照；Original_Spec 永不在此变更。
    /// </summary>
    public static class MaintenanceService
    {
        public static void Add(HwLogEntry e)
        {
            Db.InTransaction((c, tx) =>
            {
                c.Execute(
                    @"INSERT INTO Asset_Hardware_Log
                        (AssetID,ChangeType,RepairMethod,FaultDesc,ReporterEmpID,SendDate,
                         Old_Spec,New_Spec,PartName,Cost,SupplierID,HandlerUserID,ChangeTime,Remark)
                      VALUES
                        (@AssetID,@ChangeType,@RepairMethod,@FaultDesc,@ReporterEmpID,@SendDate,
                         @OldSpec,@NewSpec,@PartName,@Cost,@SupplierID,@uid,GETDATE(),@Remark)",
                    new
                    {
                        e.AssetID, e.ChangeType, e.RepairMethod, e.FaultDesc, e.ReporterEmpID, e.SendDate,
                        e.OldSpec, e.NewSpec, e.PartName, e.Cost, e.SupplierID, e.Remark,
                        uid = CurrentUser.UserID
                    }, tx);

                // 配置发生变更（提供了变更后配置）→ 同步主表当前配置
                if (!string.IsNullOrWhiteSpace(e.NewSpec))
                    c.Execute("UPDATE Asset_Info SET Current_Spec=@s, UpdateTime=GETDATE() WHERE AssetID=@a",
                        new { s = e.NewSpec, a = e.AssetID }, tx);
            });
        }
    }
}
