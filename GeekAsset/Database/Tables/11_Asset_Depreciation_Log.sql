/* =============================================================
   11_Asset_Depreciation_Log  折旧历史流水表
   财务折旧模块 2.5。三铁律之 规则C 幂等：
   (AssetID, DeprecYear, DeprecMonth) 联合唯一索引，一月只提一次。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Asset_Depreciation_Log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Asset_Depreciation_Log
    (
        LogID             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Asset_Depreciation_Log PRIMARY KEY,
        AssetID           INT           NOT NULL,               -- 资产
        DeprecYear        INT           NOT NULL,               -- 计提年份
        DeprecMonth       INT           NOT NULL,               -- 计提月份(1-12)
        MonthlyAmount     DECIMAL(18,2) NOT NULL,               -- 本月折旧额
        AccumulatedAmount DECIMAL(18,2) NOT NULL,               -- 累计折旧额
        NetValue          DECIMAL(18,2) NOT NULL,               -- 当前净值（账面剩余价值）
        GenerateUserID    INT           NULL,                   -- 生成人 UserID
        GenerateTime      DATETIME      NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT CK_Asset_Deprec_Month CHECK (DeprecMonth BETWEEN 1 AND 12),
        CONSTRAINT UQ_Asset_Deprec_AYM UNIQUE (AssetID, DeprecYear, DeprecMonth),  -- 幂等防重
        CONSTRAINT FK_Asset_Deprec_Asset FOREIGN KEY (AssetID) REFERENCES dbo.Asset_Info(AssetID)
    );
    PRINT '表 Asset_Depreciation_Log 已创建。';
END
ELSE PRINT '表 Asset_Depreciation_Log 已存在，跳过。';
GO
