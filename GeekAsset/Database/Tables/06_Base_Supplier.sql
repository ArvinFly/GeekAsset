/* =============================================================
   06_Base_Supplier  往来单位管理（供应商 / 维保商）
   基础设置模块 2.1。维护联系人、账期、售后热线。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Base_Supplier', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Base_Supplier
    (
        SupplierID     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Base_Supplier PRIMARY KEY,
        SupplierName   NVARCHAR(100) NOT NULL,                  -- 单位名称
        SupplierType   TINYINT       NOT NULL DEFAULT(1),       -- 1=供应商 2=维保商 3=两者
        ContactPerson  NVARCHAR(50)  NULL,                      -- 联系人
        Phone          NVARCHAR(50)  NULL,                      -- 售后热线/电话
        PaymentTerm    NVARCHAR(100) NULL,                      -- 账期
        Remark         NVARCHAR(200) NULL,
        IsDeleted      BIT           NOT NULL DEFAULT(0),
        CreateTime     DATETIME      NOT NULL DEFAULT(GETDATE())
    );
    PRINT '表 Base_Supplier 已创建。';
END
ELSE PRINT '表 Base_Supplier 已存在，跳过。';
GO
