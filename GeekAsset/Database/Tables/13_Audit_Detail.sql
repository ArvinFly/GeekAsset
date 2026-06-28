/* =============================================================
   13_Audit_Detail  盘点明细表
   扫码盘点模块 2.3。承载扫码结果与差异对账：
   盘盈(无账有物)/盘亏(有账无物)/信息不符。
   AssetID 为空表示扫到未知条码（盘盈）。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Audit_Detail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Audit_Detail
    (
        DetailID         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Audit_Detail PRIMARY KEY,
        TaskID           INT           NOT NULL,                -- 所属盘点任务
        AssetID          INT           NULL,                    -- 资产（盘盈时为 NULL）
        ScannedCode      NVARCHAR(100) NULL,                    -- 实扫条码
        AuditStatus      TINYINT       NOT NULL DEFAULT(1),     -- 1=未盘 2=已盘
        ResultType       TINYINT       NULL,                    -- 1正常 2盘盈 3盘亏 4信息不符
        BookEmpID        INT           NULL,                    -- 账面使用人
        ActualEmpID      INT           NULL,                    -- 实际使用人
        BookLocationID   INT           NULL,                    -- 账面位置
        ActualLocationID INT           NULL,                    -- 实际位置
        ScanTime         DATETIME      NULL,                    -- 扫码时间
        Remark           NVARCHAR(500) NULL,
        CONSTRAINT FK_Audit_Detail_Task  FOREIGN KEY (TaskID) REFERENCES dbo.Audit_Task(TaskID),
        CONSTRAINT FK_Audit_Detail_Asset FOREIGN KEY (AssetID) REFERENCES dbo.Asset_Info(AssetID)
    );
    CREATE INDEX IX_Audit_Detail_Task ON dbo.Audit_Detail(TaskID);
    PRINT '表 Audit_Detail 已创建。';
END
ELSE PRINT '表 Audit_Detail 已存在，跳过。';
GO
