/* =============================================================
   07_Base_DepreciationRule  折旧规则配置（按资产分类）
   财务折旧模块 2.5。每个分类一条规则：预计年限 + 残值率。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Base_DepreciationRule', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Base_DepreciationRule
    (
        RuleID           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Base_DepreciationRule PRIMARY KEY,
        CategoryID       INT           NOT NULL,                -- 关联资产分类（唯一）
        UsefulLifeYears  INT           NOT NULL,                -- 预计使用年限（如 PC=3, 服务器=5）
        ResidualRate     DECIMAL(5,4)  NOT NULL DEFAULT(0.0500),-- 预留残值率（默认 5%）
        CreateTime       DATETIME      NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT UQ_Base_DepreciationRule_Cat UNIQUE (CategoryID),
        CONSTRAINT CK_Base_DepreciationRule_Rate CHECK (ResidualRate >= 0 AND ResidualRate < 1),
        CONSTRAINT FK_Base_DepreciationRule_Cat FOREIGN KEY (CategoryID) REFERENCES dbo.Base_AssetCategory(CategoryID)
    );
    PRINT '表 Base_DepreciationRule 已创建。';
END
ELSE PRINT '表 Base_DepreciationRule 已存在，跳过。';
GO
