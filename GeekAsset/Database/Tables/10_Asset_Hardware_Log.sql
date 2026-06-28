/* =============================================================
   10_Asset_Hardware_Log  硬件/维保变更日志表（配置/费用变更）
   数据库 3.1 双表分离之二 + 2.4 维修维保。
   记录：换件/升级/维修；含 Old_Spec/New_Spec 防调包快照与费用。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Asset_Hardware_Log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Asset_Hardware_Log
    (
        LogID          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Asset_Hardware_Log PRIMARY KEY,
        AssetID        INT           NOT NULL,                  -- 资产
        ChangeType     NVARCHAR(20)  NOT NULL,                 -- 换件/升级/维修
        RepairMethod   TINYINT       NULL,                      -- 1=内部更换 2=原厂/第三方外修
        FaultDesc      NVARCHAR(500) NULL,                      -- 故障现象
        ReporterEmpID  INT           NULL,                      -- 报修人
        SendDate       DATE          NULL,                      -- 送修日期
        Old_Spec       NVARCHAR(500) NULL,                      -- 变更前配置快照
        New_Spec       NVARCHAR(500) NULL,                      -- 变更后配置快照
        PartName       NVARCHAR(200) NULL,                      -- 更换配件（防调包）
        Cost           DECIMAL(18,2) NOT NULL DEFAULT(0),       -- 产生费用
        SupplierID     INT           NULL,                      -- 外修单位
        HandlerUserID  INT           NULL,                      -- 经办人 UserID
        ChangeTime     DATETIME      NOT NULL DEFAULT(GETDATE()),
        Remark         NVARCHAR(500) NULL,
        CONSTRAINT CK_Asset_Hardware_Log_Type CHECK (ChangeType IN (N'换件', N'升级', N'维修')),
        CONSTRAINT FK_Asset_Hardware_Log_Asset    FOREIGN KEY (AssetID) REFERENCES dbo.Asset_Info(AssetID),
        CONSTRAINT FK_Asset_Hardware_Log_Supplier FOREIGN KEY (SupplierID) REFERENCES dbo.Base_Supplier(SupplierID)
    );
    CREATE INDEX IX_Asset_Hardware_Log_Asset ON dbo.Asset_Hardware_Log(AssetID, ChangeTime);
    PRINT '表 Asset_Hardware_Log 已创建。';
END
ELSE PRINT '表 Asset_Hardware_Log 已存在，跳过。';
GO
