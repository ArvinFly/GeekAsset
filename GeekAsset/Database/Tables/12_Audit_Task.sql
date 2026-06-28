/* =============================================================
   12_Audit_Task  盘点任务表
   扫码盘点模块 2.3。按部门/区域位置/分类生成盘点计划。
   ============================================================= */
USE [GeekAsset];
GO
IF OBJECT_ID(N'dbo.Audit_Task', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Audit_Task
    (
        TaskID        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Audit_Task PRIMARY KEY,
        TaskName      NVARCHAR(100) NOT NULL,                   -- 如《2026年中期机房资产盘点》
        ScopeType     TINYINT       NOT NULL,                   -- 1=部门 2=区域位置 3=分类
        ScopeRefID    INT           NULL,                       -- 对应 DeptID/LocationID/CategoryID
        Status        TINYINT       NOT NULL DEFAULT(1),        -- 1=进行中 2=已完成
        StartTime     DATETIME      NOT NULL DEFAULT(GETDATE()),
        EndTime       DATETIME      NULL,
        CreateUserID  INT           NULL,                       -- 创建人 UserID
        Remark        NVARCHAR(500) NULL,
        CONSTRAINT CK_Audit_Task_Scope CHECK (ScopeType IN (1,2,3))
    );
    PRINT '表 Audit_Task 已创建。';
END
ELSE PRINT '表 Audit_Task 已存在，跳过。';
GO
