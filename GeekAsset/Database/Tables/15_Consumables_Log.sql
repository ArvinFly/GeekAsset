/* =============================================================
   15_Consumables_Log  耗材出入库流水表
   耗材模块 2.6。入库加库存、领用减库存；BalanceAfter 留存操作后库存。
   注：领用扣减须在事务内带 WHERE Inventory >= @Qty 防超扣（见 3.3 并发要点）。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Consumables_Log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consumables_Log
    (
        LogID          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Consumables_Log PRIMARY KEY,
        ConsumableID   INT           NOT NULL,                  -- 耗材
        ActionType     TINYINT       NOT NULL,                  -- 1=入库 2=领用
        Quantity       INT           NOT NULL,                  -- 数量（正数）
        BalanceAfter   INT           NOT NULL,                  -- 操作后库存
        ReceiverEmpID  INT           NULL,                      -- 领用人
        OperatorUserID INT           NULL,                      -- 操作人 UserID
        ActionTime     DATETIME      NOT NULL DEFAULT(GETDATE()),
        Remark         NVARCHAR(500) NULL,
        CONSTRAINT CK_Consumables_Log_Action CHECK (ActionType IN (1,2)),
        CONSTRAINT CK_Consumables_Log_Qty CHECK (Quantity > 0),
        CONSTRAINT FK_Consumables_Log_Item FOREIGN KEY (ConsumableID) REFERENCES dbo.Consumables_Info(ConsumableID)
    );
    CREATE INDEX IX_Consumables_Log_Item ON dbo.Consumables_Log(ConsumableID, ActionTime);
    PRINT '表 Consumables_Log 已创建。';
END
ELSE PRINT '表 Consumables_Log 已存在，跳过。';
GO
