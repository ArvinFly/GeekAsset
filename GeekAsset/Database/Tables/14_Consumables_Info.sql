/* =============================================================
   14_Consumables_Info  低值易耗品主表（含微仓库存）
   耗材模块 2.6。Inventory 当前库存，SafetyStock 安全库存阈值。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Consumables_Info', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consumables_Info
    (
        ConsumableID    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Consumables_Info PRIMARY KEY,
        ConsumableName  NVARCHAR(100) NOT NULL,                 -- 品名（鼠标/网线/水晶头）
        Spec            NVARCHAR(100) NULL,                     -- 规格
        Unit            NVARCHAR(20)  NULL,                     -- 单位（个/箱/卷）
        CategoryName    NVARCHAR(50)  NULL,                     -- 耗材类目
        Inventory       INT           NOT NULL DEFAULT(0),      -- 当前库存
        SafetyStock     INT           NOT NULL DEFAULT(0),      -- 安全库存阈值（低于则预警）
        IsDeleted       BIT           NOT NULL DEFAULT(0),
        CreateTime      DATETIME      NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT CK_Consumables_Inv CHECK (Inventory >= 0)     -- 库存不可为负
    );
    PRINT '表 Consumables_Info 已创建。';
END
ELSE PRINT '表 Consumables_Info 已存在，跳过。';
GO
