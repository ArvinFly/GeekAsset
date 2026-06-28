/* =============================================================
   09_Asset_Flow_Log  流转日志表（人/位置流转）
   数据库 3.1 双表分离之一。只记录：领用/退还/借用/归还/调拨。
   含生命周期终态报废。不可逆追加记录，支撑“设备用过哪些人/最终去向”履历。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Asset_Flow_Log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Asset_Flow_Log
    (
        LogID            BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Asset_Flow_Log PRIMARY KEY,
        AssetID          INT           NOT NULL,                -- 资产
        ActionType       NVARCHAR(20)  NOT NULL,               -- 领用/退还/借用/归还/调拨
        FromEmpID        INT           NULL,                    -- 来源归属人
        ToEmpID          INT           NULL,                    -- 目标归属人
        FromLocationID   INT           NULL,                    -- 来源位置（调拨）
        ToLocationID     INT           NULL,                    -- 目标位置（调拨）
        ExpectReturnDate DATE          NULL,                    -- 短期借用预计归还时间（超时催还）
        ActualReturnDate DATE          NULL,                    -- 实际归还时间
        OperatorUserID   INT           NULL,                    -- 操作人 UserID
        ActionTime       DATETIME      NOT NULL DEFAULT(GETDATE()),
        Remark           NVARCHAR(500) NULL,
        CONSTRAINT CK_Asset_Flow_Log_Action CHECK (ActionType IN (N'领用', N'退还', N'借用', N'归还', N'调拨', N'报废')),
        CONSTRAINT FK_Asset_Flow_Log_Asset FOREIGN KEY (AssetID) REFERENCES dbo.Asset_Info(AssetID)
    );
    CREATE INDEX IX_Asset_Flow_Log_Asset ON dbo.Asset_Flow_Log(AssetID, ActionTime);
    PRINT '表 Asset_Flow_Log 已创建。';
END
ELSE PRINT '表 Asset_Flow_Log 已存在，跳过。';
GO
